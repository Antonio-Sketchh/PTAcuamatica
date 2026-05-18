-- Elimina registros históricos del año 2024 hacia atrás en lotes de 10,000 filas.
-- Ejecutar en horario de baja carga para minimizar impacto en el transaction log.

DECLARE @BatchSize INT = 10000;
DECLARE @RowsDeleted INT = 1;

WHILE @RowsDeleted > 0
BEGIN
    DELETE TOP (@BatchSize)
    FROM [dbo].[LoginTrace]
    WHERE [Date] < '2025-01-01';

    SET @RowsDeleted = @@ROWCOUNT;
END
