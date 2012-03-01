DROP TABLE IF EXISTS [BudgetTemplateItem];
CREATE TABLE [BudgetTemplateItem] (
  [Id]  INTEGER PRIMARY KEY AUTOINCREMENT,   
  [TemplateTypeName] VARCHAR(500),
  [ForeignId] INTEGER,
  [Value] NUMERIC,
  [Description] VARCHAR(500),
  [MonthDay] INTEGER,
  [RepeatInterval] INTEGER, -- month unit - i.e. 1 - each month, 3 - each quarter
  [StartDate] DATETIME, -- date from which repeat interval is counted
  [IsActive] INTEGER(1) DEFAULT 1
);

DROP TABLE IF EXISTS [BudgetTemplateHistory];
CREATE TABLE [BudgetTemplateHistory] (
  [Id]  INTEGER PRIMARY KEY AUTOINCREMENT,
  [BudgetTemplateItemId] INTEGER,
  [BudgetId] INTEGER,
  [Date] DATETIME -- date on witch item was tested for execution (could be skipped because of BudgetTemplateItem.RepeatInterval)
);

UPDATE [Configuration] SET [Value] = '3' WHERE [Key] = 'DatabaseVersion';