-- Índice optimizado para reportes de auditoría filtrados por rango de fecha y usuario.
-- ONLINE = ON permite crearlo sin bloquear la tabla (crítico con 5M de registros).

CREATE NONCLUSTERED INDEX [IX_LoginTrace_Date_Username]
ON [dbo].[LoginTrace] ([Date] ASC, [Username] ASC)
WITH (ONLINE = ON);
