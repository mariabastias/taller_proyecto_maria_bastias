using TruequeTextil.Features.ProponerTrueque.Interfaces;
using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.ProponerTrueque;

public class ProponerTruequeService : IProponerTruequeService
{
    private readonly IProponerTruequeRepository _repository;
    private readonly ILogger<ProponerTruequeService> _logger;

    private const int MAXIMO_PROPUESTAS_POR_PRENDA = 3; // RF-09

    public ProponerTruequeService(
        IProponerTruequeRepository repository,
        ILogger<ProponerTruequeService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    // RF-09: Crear propuesta con limite de 3 activas por prenda
    public async Task<(bool Exito, string Mensaje, int PropuestaId)> CrearPropuesta(
        int usuarioProponenteId, int prendaOfrecidaId, int prendaSolicitadaId, string? mensaje = null)
    {
        try
        {
            // Validar prenda ofrecida
            var prendaOfrecida = await _repository.ObtenerPrendaPorId(prendaOfrecidaId);
            if (prendaOfrecida == null)
            {
                return (false, "La prenda que ofreces no existe", 0);
            }

            if (prendaOfrecida.UsuarioId != usuarioProponenteId)
            {
                return (false, "Solo puedes ofrecer tus propias prendas", 0);
            }

            if (prendaOfrecida.EstadoPublicacionId != 1)
            {
                return (false, "Tu prenda ya no esta disponible para trueque", 0);
            }

            // Validar prenda solicitada
            var prendaSolicitada = await _repository.ObtenerPrendaPorId(prendaSolicitadaId);
            if (prendaSolicitada == null)
            {
                return (false, "La prenda que solicitas no existe", 0);
            }

            if (prendaSolicitada.EstadoPublicacionId != 1)
            {
                return (false, "La prenda que solicitas ya no esta disponible", 0);
            }

            if (prendaSolicitada.UsuarioId == usuarioProponenteId)
            {
                return (false, "No puedes proponer trueque con tus propias prendas", 0);
            }

            // RF-09: Verificar limite de 3 propuestas activas por prenda solicitada
            var propuestasActivasSolicitada = await _repository.ContarPropuestasActivasPorPrenda(prendaSolicitadaId);
            if (propuestasActivasSolicitada >= MAXIMO_PROPUESTAS_POR_PRENDA)
            {
                return (false, $"Esta prenda ya tiene {MAXIMO_PROPUESTAS_POR_PRENDA} propuestas activas. Intenta mas tarde.", 0);
            }

            // RF-09: Verificar limite de 3 propuestas activas por prenda ofrecida
            var propuestasActivasOfrecida = await _repository.ContarPropuestasActivasPorPrenda(prendaOfrecidaId);
            if (propuestasActivasOfrecida >= MAXIMO_PROPUESTAS_POR_PRENDA)
            {
                return (false, $"Tu prenda ya tiene {MAXIMO_PROPUESTAS_POR_PRENDA} propuestas activas. Espera respuesta o cancela alguna.", 0);
            }

            // Verificar que no exista propuesta duplicada
            var existePropuesta = await _repository.ExistePropuestaActiva(prendaOfrecidaId, prendaSolicitadaId);
            if (existePropuesta)
            {
                return (false, "Ya existe una propuesta activa para estas prendas", 0);
            }

            // Crear propuesta
            var propuesta = new PropuestaTrueque
            {
                UsuarioProponenteId = usuarioProponenteId,
                MensajeAcompanante = mensaje ?? string.Empty,
                Prioridad = 1,
                EsContraoferta = false
            };

            var propuestaId = await _repository.CrearPropuesta(propuesta, prendaOfrecidaId, prendaSolicitadaId);

            _logger.LogInformation(
                "Propuesta de trueque {PropuestaId} creada: usuario {UsuarioId} ofrece prenda {PrendaOfrecida} por prenda {PrendaSolicitada}. Expira en 7 dias.",
                propuestaId, usuarioProponenteId, prendaOfrecidaId, prendaSolicitadaId);

            return (true, "Propuesta enviada exitosamente. El usuario tiene 7 dias para responder.", propuestaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear propuesta de trueque");
            return (false, "Error al procesar la propuesta", 0);
        }
    }

    public async Task<List<Prenda>> ObtenerMisPrendasDisponibles(int usuarioId)
    {
        try
        {
            return await _repository.ObtenerPrendasDisponiblesUsuario(usuarioId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener prendas disponibles del usuario {UsuarioId}", usuarioId);
            return new List<Prenda>();
        }
    }

    public async Task<Prenda?> ObtenerPrendaDestino(int prendaId)
    {
        try
        {
            return await _repository.ObtenerPrendaPorId(prendaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener prenda destino {PrendaId}", prendaId);
            return null;
        }
    }

    // RF-09: Verificar propuestas activas para una prenda
    public async Task<(int Activas, int Maximo)> ObtenerEstadoPropuestasPrenda(int prendaId)
    {
        try
        {
            var activas = await _repository.ContarPropuestasActivasPorPrenda(prendaId);
            return (activas, MAXIMO_PROPUESTAS_POR_PRENDA);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al contar propuestas activas para prenda {PrendaId}", prendaId);
            return (0, MAXIMO_PROPUESTAS_POR_PRENDA);
        }
    }
}
