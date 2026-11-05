# Servicio de Alertas SmartPalm

Microservicio del BC-03 Alertas y Notificaciones. Consume violaciones de umbrales desde Ingestion, suprime duplicados durante 30 minutos, persiste el ciclo de vida de las alertas, envía notificaciones push críticas mediante Firebase Cloud Messaging y publica eventos de integración de alertas.

## Rol en la plataforma

Alert es la capa de reacción ante valores anómalos de los sensores. Ingestion evalúa los umbrales y emite una violación; Alert resuelve la configuración de notificaciones del usuario afectado, suprime repeticiones, almacena el estado de la alerta y opcionalmente envía una notificación push crítica. Los PalmGrowers consultan las alertas resultantes desde los clientes web o móvil a través del API Gateway.

## Relaciones de integración

- **Ingestion Service:** fuente de eventos de violación de umbrales; las lecturas y definiciones de umbrales permanecen en Ingestion.
- **Crop Service:** proporciona las proyecciones locales necesarias para resolver los sectores y usuarios destinatarios.
- **Identity Service:** proporciona eventos de usuarios/suscripciones utilizados por las proyecciones; Alert no es dueño de las cuentas de usuario.
- **Firebase Cloud Messaging:** canal externo opcional para entregar notificaciones críticas.
- **API Gateway y clientes web/móvil:** exponen consultas de alertas, confirmación/cambios de estado y configuración de notificaciones por sensor.

## Ejecución local

1. Copia `.env.example` como `.env` y configura la clave JWT compartida.
2. Ejecuta `docker compose --profile isolated up -d` para iniciar PostgreSQL y una instancia aislada de RabbitMQ, u omite el perfil para utilizar el broker de la plataforma.
3. Ejecuta `dotnet run --project SmartPalm.AlertService.API`.

La API escucha por defecto en el puerto `5105`. Los endpoints de disponibilidad y estado son `/health/ready` y `/health/live`.

## Límite de propiedad

Este servicio es dueño de las alertas, la configuración de notificaciones de usuarios, las decisiones de supresión, las entregas de notificaciones, las proyecciones de Crop necesarias para resolver destinatarios, los mensajes de inbox y los mensajes de outbox. No es dueño de las lecturas de sensores, umbrales, plantaciones, sectores, afiliaciones, usuarios ni suscripciones.
