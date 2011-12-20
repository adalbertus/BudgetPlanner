DROP TABLE IF EXISTS [BudgetCalculatorItem];
CREATE TABLE [BudgetCalculatorItem] (
  [Id]  INTEGER PRIMARY KEY AUTOINCREMENT,
  [BudgetCalculatorEquationId] INTEGER NOT NULL,
  [Name] VARCHAR(250),  
  [ValueTypeName] VARCHAR(500),
  [OperatorTypeName] VARCHAR(500),
  [ForeignId] INTEGER,
  [Value] NUMERIC,
  [Position] INTEGER DEFAULT 0
);

DROP TABLE IF EXISTS [BudgetCalculatorEquation];
CREATE TABLE [BudgetCalculatorEquation] (
  [Id]  INTEGER PRIMARY KEY AUTOINCREMENT,
  [Name] VARCHAR(250),
  [IsVisible] INTEGER(1) DEFAULT 1,
  [Position] INTEGER DEFAULT 0
);

UPDATE [Configuration] SET [Value] = 2 WHERE [Key] = 'DatabaseVersion';