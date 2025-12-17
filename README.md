# Trueque Textil - Chile

## Descripción del Proyecto
  Trueque Textil es un sistema web desarrollado en Blazor Server que permite el intercambio de prendas de vestir entre usuarios en Chile, transformando la práctica tradicional del trueque —limitada a ferias presenciales y grupos informales— en una plataforma digital permanente, estructurada y confiable.

  La aplicación está orientada a fomentar la economía circular, promoviendo la reutilización de ropa mediante un sistema de publicaciones, negociación estructurada, comunicación interna y mecanismos de reputación verificables entre usuarios.

## Problema que Aborda
  Chile enfrenta una problemática ambiental relevante asociada al consumo textil, generando más de 572.000 toneladas de residuos textiles al año, con un alto nivel de descarte prematuro de prendas en buen estado.

  Las iniciativas actuales de trueque presentan limitaciones significativas: ausencia de trazabilidad, falta de confianza entre usuarios, inexistencia de historial de intercambios y escasos mecanismos de verificación.

  Trueque Textil responde a esta problemática mediante una solución digital que organiza, registra y valida el proceso de intercambio, facilitando decisiones informadas y fortaleciendo comunidades de consumo responsable.

## Funcionalidades Principales
1) Registro y autenticación segura de usuarios
2) Gestión de perfiles públicos con información y reputación
3) Publicación de prendas con descripción detallada y carga de imágenes
4) Exploración y búsqueda avanzada por tipo, talla, estado y ubicación
5) Sistema estructurado de propuestas y negociación de trueques
6) Chat interno asociado a propuestas activas
7) Sistema de evaluación y reputación post-intercambio
8) Historial verificable de transacciones
9) Sistema de notificaciones en tiempo real
10) Panel de administración y moderación

## Tecnologías Utilizadas
- **Frontend y Backend**: Blazor Server (C# .NET 10)
- **Lenguaje**: C#
- **Acceso a Datos**: ADO.NET puro
- **Base de datos**: SQL Server
- **Comunicación en tiempo real**: SignalR
- **Control de versiones**: GitHub
- **Gestión del proyecto**: GitHub Projects (Kanban)


## Metodología de Desarrollo
El proyecto fue desarrollado utilizando un enfoque ágil híbrido, combinando:
 
  - Lean Development para priorización de valor
  - Kanban personal para control visual del flujo de trabajo
  - Extreme Programming (XP) adaptado, enfocado en simplicidad, refactorización continua y calidad técnica

La planificación y seguimiento se gestionan mediante un **tablero Kanban** para gestión visual de tareas en GitHub Projects:

[**🔗 Ver Tablero Kanban del Proyecto**](https://github.com/users/mariabastias/projects/1/views/1)

## Arquitectura del Sistema

  El sistema está diseñado bajo una arquitectura modular por características (Vertical Slices), donde cada funcionalidad encapsula su propia lógica de presentación, negocio y acceso a datos.

  Se mantiene una separación clara de responsabilidades:

    - UI (Blazor):  presentación y experiencia de usuario
    - Servicios: lógica de negocio y reglas del sistema
    - Repositorios: acceso directo y controlado a la base de datos

  Esta arquitectura favorece la mantenibilidad, escalabilidad y trazabilidad del sistema.

## Estructura del Repositorio
- `/documentos` - Informes PDF y documentación
- `/diagramas` - Diagrama de actividades
- `/codigo` - Código fuente Blazor Server
- `/assets` - Imágenes y recursos 
- `/database` - Base de datos y scripts
- `/desing` - Casos de uso, diagrama MER, wireframes y wireflows

## Estado del Proyecto

  📌 Proyecto académico desarrollado como Informe Final de Taller de Proyecto de Especialidad.
El sistema se encuentra implementado a nivel de diseño completo y prototipo funcional.

## Autora
María Constanza Bastías Sanhueza
Carrera: Programación y Análisis de Sistemas
Año: 2025
Taller de Proyecto de Especialidad, del Instituto AIEP 

## Licencia
Este proyecto está bajo la Licencia MIT - ver archivo [LICENSE](LICENSE) para detalles.

## Informe Detallado
[Descargar desde Google Drive](https://drive.google.com/file/d/1ffHtDpQdV1WJnnw-Rgt3rLC5xRZPcuFX/view?usp=sharing)
*Nota: El PDF está alojado en Google Drive para garantizar su correcta visualización. Igualmente, se encuentra disponible en la carpeta "Documentos"*

---

### In Memorian

Este proyecto está dedicado a la memoria de **Aaron Swartz**,  
como reconocimiento a su visión de la tecnología  al servicio de la sociedad, 
el acceso al conocimiento  
y la construcción de sistemas con impacto social.

