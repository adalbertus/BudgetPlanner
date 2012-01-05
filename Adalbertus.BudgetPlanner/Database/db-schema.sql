DROP TABLE IF EXISTS [Configuration];
CREATE TABLE [Configuration] (
  [Key] VARCHAR(64) PRIMARY KEY, 
  [IsActive] INTEGER(1) DEFAULT 1,
  [Value] TEXT,
  [Decription] TEXT
);

DROP TABLE IF EXISTS [Budget];
CREATE TABLE [Budget] (
  [Id] INTEGER PRIMARY KEY AUTOINCREMENT, 
  [DateFrom] DATETIME, 
  [DateTo] DATETIME,
  [TransferedValue] NUMERIC);

DROP TABLE IF EXISTS [BudgetPlan];
CREATE TABLE [BudgetPlan] (
  [Id]  INTEGER PRIMARY KEY AUTOINCREMENT,   
  [BudgetId] INTEGER NOT NULL,
  [CashFlowId] INTEGER NOT NULL,
  [Value] NUMERIC,
  [Description] VARCHAR(250)
);
DROP TABLE IF EXISTS [CashFlow];
CREATE TABLE [CashFlow] (
  [Id]  INTEGER PRIMARY KEY AUTOINCREMENT,   
  [CashFlowGroupId] INTEGER NOT NULL,
  [Name] VARCHAR(250),
  [Description] VARCHAR(250)
);

DROP TABLE IF EXISTS [CashFlowGroup];
CREATE TABLE [CashFlowGroup] (
  [Id]  INTEGER PRIMARY KEY AUTOINCREMENT,   
  [Name] VARCHAR(250),
  [Description] VARCHAR(250),
  [IsReadOnly] INTEGER(1) DEFAULT 0,
  [Position] INTEGER DEFAULT 0
);

DROP TABLE IF EXISTS [Expense];
CREATE TABLE [Expense] (
  [Id]  INTEGER PRIMARY KEY AUTOINCREMENT,   
  [CashFlowId] INTEGER NOT NULL,
  [BudgetId] INTEGER NOT NULL,
  [Value] NUMERIC, 
  [Date] DATETIME, 
  [Description] VARCHAR(250)
);
DROP TABLE IF EXISTS [Income];
CREATE TABLE [Income] (
  [Id]  INTEGER PRIMARY KEY AUTOINCREMENT,
  [Name] VARCHAR(500)
);
DROP TABLE IF EXISTS [IncomeValue];
CREATE TABLE [IncomeValue] (
  [Id]  INTEGER PRIMARY KEY AUTOINCREMENT,   
  [IncomeId] INTEGER NOT NULL,
  [BudgetId] INTEGER, -- can be feeded from budget or taken to budget or from other source
  [Value] NUMERIC,
  [Date] DATETIME,
  [Description] VARCHAR(250)
);
DROP TABLE IF EXISTS [Saving];
CREATE TABLE [Saving] (
  [Id]  INTEGER PRIMARY KEY AUTOINCREMENT,
  [CashFlowId] INTEGER NOT NULL,
  [Name] VARCHAR(250),
  [Description] VARCHAR(250)
);
DROP TABLE IF EXISTS [SavingValue];
CREATE TABLE [SavingValue] (
  [Id]  INTEGER PRIMARY KEY AUTOINCREMENT,   
  [SavingId] INTEGER NOT NULL,
  [BudgetId] INTEGER, -- Saving taken to Budget
  [ExpenseId] INTEGER, -- Saving given by expense
  [Value] NUMERIC,  -- can be negative (if BudgetId IS NOT NULL)
  [Date] DATETIME,
  [Description] VARCHAR(250)
);

DROP TABLE IF EXISTS [BudgetCalculatorItem];
CREATE TABLE [BudgetCalculatorItem] (
  [Id]  INTEGER PRIMARY KEY AUTOINCREMENT,
  [BudgetCalculatorEquationId] INTEGER NOT NULL,
  [Name] VARCHAR(250),  
  [ValueTypeName] VARCHAR(500),
  [OperatorTypeName] VARCHAR(500),
  [ForeignId] INTEGER,
  [Value] NUMERIC,
  [Text] VARCHAR(250),
  [Position] INTEGER DEFAULT 0
);

DROP TABLE IF EXISTS [BudgetCalculatorEquation];
CREATE TABLE [BudgetCalculatorEquation] (
  [Id]  INTEGER PRIMARY KEY AUTOINCREMENT,
  [Name] VARCHAR(250),
  [IsVisible] INTEGER(1) DEFAULT 1,
  [Position] INTEGER DEFAULT 0
);

DROP TABLE IF EXISTS [Note];
CREATE TABLE [Note] (
  [Id]  INTEGER PRIMARY KEY AUTOINCREMENT,
  [Text] TEXT
);
