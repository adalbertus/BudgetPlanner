using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adalbertus.BudgetPlanner.Models;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace Adalbertus.BudgetPlanner.Services
{
    public class ExportService : IExportService
    {
        #region Private fields
        #endregion Private fields

        #region Constructors
        #endregion Constructors

        #region Properties
        #endregion Properties

        #region Public methods
        public void ExportExpenses(string fileName, string workbookName, IEnumerable<Expense> expenses)
        {
            var file = CreateFile(fileName);
            using (var excelPackage = new ExcelPackage(file))
            {
                var workSheet = excelPackage.Workbook.Worksheets.Add(workbookName);
                CreateExpensesWorksheet(workSheet, expenses, excelPackage);

                excelPackage.Save();
            }
        }

        public void ExportBudget(string fileName, IEnumerable<Budget> budgets)
        {
            var file = CreateFile(fileName);
            var totalExpenses = new List<Expense>();
            foreach (var budget in budgets)
            {
                totalExpenses.AddRange(budget.Expenses);
            };

            using (var excelPackage = new ExcelPackage(file))
            {
                var budgetPlanWorksheet = excelPackage.Workbook.Worksheets.Add("Plan");
                var expensesWorksheet = excelPackage.Workbook.Worksheets.Add("Realizacja");
                var expensesAddressLookup = CreateExpensesWorksheet(expensesWorksheet, totalExpenses, excelPackage);
                CreateBudgetPlanWorksheet(budgetPlanWorksheet, budgets, excelPackage, expensesAddressLookup);
                excelPackage.Save();
            }
        }
        #endregion Public methods

        #region Private methods
        #region Budget plan worksheet
        private static void CreateBudgetPlanWorksheet(ExcelWorksheet workSheet, IEnumerable<Budget> budgets, ExcelPackage excelPackage, Dictionary<string, List<string>> expensesAddressLookup)
        {
            var columnNames = new string[]
                {
                    "Budżet",               // 1
                    "Grupa wydatków",       // 2
                    "Kategoria wydatków",   // 3
                    "Kwota",                // 4
                    "Opis",                 // 5
                };
            CreateWorksheetHeader(workSheet, columnNames, 1, 2);
            //                                                     7            8          9            10              11               12         13           14
            CreateWorksheetHeader(workSheet, new string[] { "Grupa wydatków", "Suma", "Realizacja", "Pozostało", "Kategoria wydatków", "Suma", "Realizacja", "Pozostało" }, 7, 2);

            int rowIndex = 3;
            int lastGroupIndex = 0;
            int lastCategoryIndex = 0;
            foreach (var budget in budgets)
            {
                for (int i = 0; i < budget.BudgetPlanItems.Count; i++)
                {
                    var budgetPlanItem = CreateBudgetPlanSummaryTable(workSheet, rowIndex, ref lastGroupIndex, ref lastCategoryIndex, budget, i, expensesAddressLookup);

                    workSheet.Cells[rowIndex, 1].Value = string.Format("{0:yyyy-MM-dd} - {1:yyyy-MM-dd}", budget.DateFrom, budget.DateTo);
                    workSheet.Cells[rowIndex, 2].Value = budgetPlanItem.CashFlow.GroupName;
                    workSheet.Cells[rowIndex, 3].Value = budgetPlanItem.CashFlow.Name;
                    workSheet.Cells[rowIndex, 4].Value = budgetPlanItem.Value;
                    workSheet.Cells[rowIndex, 5].Value = budgetPlanItem.Description;

                    var borderRange = workSheet.Cells[rowIndex, 1, rowIndex, workSheet.Dimension.End.Column];
                    if (i + 1 < budget.BudgetPlanItems.Count)
                    {
                        if (budgetPlanItem.CashFlow.Group.Id != budget.BudgetPlanItems[i + 1].CashFlow.Group.Id)
                        {
                            borderRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        }
                    }
                    else
                    {
                        borderRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    }
                    rowIndex++;
                }
            }

            AlterateAndAutoFitRowsColumns(workSheet, 3, rowIndex - 1, columnNames.Length);

            CreateBudgetPlanWorksheetSummary(workSheet, rowIndex);

            FormatToCurrency(workSheet.Column(4));
            FormatToCurrency(workSheet.Cells[1, 8, workSheet.Dimension.End.Row, 10]);
            FormatToCurrency(workSheet.Cells[1, 12, workSheet.Dimension.End.Row, 14]);
            SetBackgroundColor(workSheet.Column(6), Color.White);

            workSheet.View.FreezePanes(3, 1);

            using (var range = workSheet.Cells[workSheet.Dimension.Address])
            {
                range.AutoFitColumns(10);
                var negativeValueFormatting = workSheet.ConditionalFormatting.AddLessThan(new ExcelAddress(range.Address));
                negativeValueFormatting.Formula = "0";
                negativeValueFormatting.Style.Fill.PatternType = ExcelFillStyle.Solid;
                negativeValueFormatting.Style.Fill.BackgroundColor.Color = Color.LightPink;
            }
        }

        private static void CreateBudgetPlanWorksheetSummary(ExcelWorksheet workSheet, int rowIndex)
        {
            workSheet.Cells[1, 3].Value = "Suma";
            workSheet.Cells[1, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            workSheet.Cells[1, 4].Formula = string.Format("SUBTOTAL(9,D{0}:D{1})", 3, rowIndex - 1);
            SetBackgroundColor(workSheet.Cells[1, 4], Color.LightGreen);
            workSheet.Cells[1, 7].Value = "Suma"; // Suma, Realizacja, Pozostało
            workSheet.Cells[1, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            workSheet.Cells[1, 8].Formula = string.Format("SUBTOTAL(9,H{0}:H{1})", 2, rowIndex - 1);
            workSheet.Cells[1, 9].Formula = string.Format("SUBTOTAL(9,I{0}:I{1})", 2, rowIndex - 1);
            workSheet.Cells[1, 10].Formula = string.Format("SUBTOTAL(9,J{0}:J{1})", 2, rowIndex - 1);

            workSheet.Cells[1, 11].Value = "Suma"; // Suma, Realizacja, Pozostało
            workSheet.Cells[1, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            workSheet.Cells[1, 12].Formula = string.Format("SUBTOTAL(9,L{0}:L{1})", 2, rowIndex - 1);
            workSheet.Cells[1, 13].Formula = string.Format("SUBTOTAL(9,M{0}:M{1})", 2, rowIndex - 1);
            workSheet.Cells[1, 14].Formula = string.Format("SUBTOTAL(9,N{0}:N{1})", 2, rowIndex - 1);
            SetBackgroundColor(workSheet.Cells[1, 8, 1, 10], Color.LightGreen);
            SetBackgroundColor(workSheet.Cells[1, 12, 1, 14], Color.LightGreen);

            workSheet.Cells[rowIndex, 3].Value = "Suma";
            workSheet.Cells[rowIndex, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            workSheet.Cells[rowIndex, 4].Formula = string.Format("SUBTOTAL(9,D{0}:D{1})", 2, rowIndex - 1);
            SetBackgroundColor(workSheet.Cells[rowIndex, 4], Color.LightGreen);

            workSheet.Cells[rowIndex, 7].Value = "Suma"; // Suma, Realizacja, Pozostało
            workSheet.Cells[rowIndex, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            workSheet.Cells[rowIndex, 8].Formula = string.Format("SUBTOTAL(9,H{0}:H{1})", 2, rowIndex - 1);
            workSheet.Cells[rowIndex, 9].Formula = string.Format("SUBTOTAL(9,I{0}:I{1})", 2, rowIndex - 1);
            workSheet.Cells[rowIndex, 10].Formula = string.Format("SUBTOTAL(9,J{0}:J{1})", 2, rowIndex - 1);

            workSheet.Cells[rowIndex, 11].Value = "Suma"; // Suma, Realizacja, Pozostało
            workSheet.Cells[rowIndex, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            workSheet.Cells[rowIndex, 12].Formula = string.Format("SUBTOTAL(9,L{0}:L{1})", 2, rowIndex - 1);
            workSheet.Cells[rowIndex, 13].Formula = string.Format("SUBTOTAL(9,M{0}:M{1})", 2, rowIndex - 1);
            workSheet.Cells[rowIndex, 14].Formula = string.Format("SUBTOTAL(9,N{0}:N{1})", 2, rowIndex - 1);
            SetBackgroundColor(workSheet.Cells[rowIndex, 8, rowIndex, 10], Color.LightGreen);
            SetBackgroundColor(workSheet.Cells[rowIndex, 12, rowIndex, 14], Color.LightGreen);

            workSheet.Row(1).Style.Font.Bold = true;
            workSheet.Row(rowIndex).Style.Font.Bold = true;
        }

        private static BudgetPlan CreateBudgetPlanSummaryTable(ExcelWorksheet workSheet, int rowIndex, ref int lastGroupIndex, ref int lastCategoryIndex, Budget budget, int i, Dictionary<string, List<string>> expensesAddressLookup)
        {
            var budgetPlanItem = budget.BudgetPlanItems[i];
            var previousBudgetPlanItem = budgetPlanItem;
            if (i > 0)
            {
                previousBudgetPlanItem = budget.BudgetPlanItems[i - 1];
            }

            var lastElement = i == budget.BudgetPlanItems.Count - 1;

            if (previousBudgetPlanItem.CashFlowId == budgetPlanItem.CashFlowId)
            {
                lastCategoryIndex = rowIndex;
            }
            else
            {
                lastCategoryIndex = FillCategorySummaryRows(workSheet, rowIndex, lastCategoryIndex, budget, previousBudgetPlanItem, expensesAddressLookup);
            }

            if (previousBudgetPlanItem.CashFlow.Group.Id == budgetPlanItem.CashFlow.Group.Id)
            {
                lastGroupIndex = rowIndex;
            }
            else
            {
                lastGroupIndex = FillGroupSummaryRows(workSheet, rowIndex, lastGroupIndex, budget, previousBudgetPlanItem);
            }

            if (lastElement)
            {
                FillCategorySummaryRows(workSheet, rowIndex, lastCategoryIndex, budget, budgetPlanItem, expensesAddressLookup);
                FillGroupSummaryRows(workSheet, rowIndex, lastGroupIndex, budget, budgetPlanItem);
            }

            return budgetPlanItem;
        }

        private static int FillGroupSummaryRows(ExcelWorksheet workSheet, int rowIndex, int lastGroupIndex, Budget budget, BudgetPlan previousBudgetPlanItem)
        {
            var groupRows = budget.BudgetPlanItems.Count(x => x.CashFlow.Group.Id == previousBudgetPlanItem.CashFlow.Group.Id);
            var firstGroupIndex = lastGroupIndex - (groupRows - 1);
            workSheet.Cells[lastGroupIndex, 7].Formula = string.Format("B{0}", lastGroupIndex);
            if (lastGroupIndex > firstGroupIndex)
            {
                // Grupa wydatków
                workSheet.Cells[firstGroupIndex, 7, lastGroupIndex, 7].Merge = true;
                workSheet.Cells[firstGroupIndex, 7, lastGroupIndex, 7].Formula = string.Format("B{0}", lastGroupIndex);

                // Suma
                workSheet.Cells[firstGroupIndex, 8, firstGroupIndex, 8].Formula = string.Format("SUM(L{0}:L{1})", firstGroupIndex, lastGroupIndex);
                workSheet.Cells[firstGroupIndex, 8, lastGroupIndex, 8].Merge = true;

                // Realizacja
                workSheet.Cells[firstGroupIndex, 9, firstGroupIndex, 9].Formula = string.Format("SUM(M{0}:M{1})", firstGroupIndex, lastGroupIndex);
                workSheet.Cells[firstGroupIndex, 9, lastGroupIndex, 9].Merge = true;

                // Pozostało
                workSheet.Cells[firstGroupIndex, 10, firstGroupIndex, 10].Formula = string.Format("SUM(N{0}:N{1})", firstGroupIndex, lastGroupIndex);
                workSheet.Cells[firstGroupIndex, 10, lastGroupIndex, 10].Merge = true;
            }
            else
            {
                workSheet.Cells[lastGroupIndex, 8].Formula = string.Format("SUM(L{0}:L{1})", firstGroupIndex, lastGroupIndex);
                workSheet.Cells[lastGroupIndex, 9].Formula = string.Format("SUM(M{0}:M{1})", firstGroupIndex, lastGroupIndex);
                workSheet.Cells[lastGroupIndex, 10].Formula = string.Format("SUM(N{0}:N{1})", firstGroupIndex, lastGroupIndex);
            }

            lastGroupIndex = rowIndex;
            return lastGroupIndex;
        }

        private static int FillCategorySummaryRows(ExcelWorksheet workSheet, int rowIndex, int lastCategoryIndex, Budget budget, BudgetPlan previousBudgetPlanItem, Dictionary<string, List<string>> expensesAddressLookup)
        {
            var categoryRows = budget.BudgetPlanItems.Count(x => x.CashFlowId == previousBudgetPlanItem.CashFlowId);
            var firstCategoryIndex = lastCategoryIndex - (categoryRows - 1);

            // Kategoria wydatków
            workSheet.Cells[lastCategoryIndex, 11].Formula = string.Format("C{0}", lastCategoryIndex);
            // Suma
            workSheet.Cells[lastCategoryIndex, 12].Formula = string.Format("SUM(D{0}:D{1})", firstCategoryIndex, lastCategoryIndex);

            // Realizacja: find expenses for this category and budget
            string key = GetExpenseLookupKey(budget.Id, previousBudgetPlanItem.CashFlow.Group.Id, previousBudgetPlanItem.CashFlowId);
            if (expensesAddressLookup.ContainsKey(key))
            {
                workSheet.Cells[lastCategoryIndex, 13].Formula = string.Format("SUM({0})", string.Join(",", expensesAddressLookup[key]));
            }
            else
            {
                workSheet.Cells[lastCategoryIndex, 13].Value = 0;
            }

            // Pozostało
            workSheet.Cells[lastCategoryIndex, 14].Formula = string.Format("L{0} - M{0}", lastCategoryIndex);

            lastCategoryIndex = rowIndex;
            return lastCategoryIndex;
        }
        #endregion Budget plan worksheet

        #region Expenses worksheet
        private static Dictionary<string, List<string>> CreateExpensesWorksheet(ExcelWorksheet workSheet, IEnumerable<Expense> expenses, ExcelPackage excelPackage)
        {
            var columnNames = new string[]
                    {
                        "L.p.",
                        "Grupa wydatków",
                        "Kategoria wydatków",
                        "Data",
                        "Kwota",
                        "Opis"
                    };

            CreateWorksheetHeader(workSheet, columnNames, 1, 2);

            var expenseAddressLookup = new Dictionary<string, List<string>>();

            int rowIndex = 3;
            foreach (var expense in expenses)
            {
                //workSheet
                workSheet.Cells[rowIndex, 1].Value = rowIndex - 1;
                workSheet.Cells[rowIndex, 2].Value = expense.Flow.GroupName;
                workSheet.Cells[rowIndex, 3].Value = expense.Flow.Name;
                workSheet.Cells[rowIndex, 4].Value = expense.Date;
                workSheet.Cells[rowIndex, 5].Value = expense.Value;
                workSheet.Cells[rowIndex, 6].Value = expense.Description;

                var key = GetExpenseLookupKey(expense.BudgetId, expense.CashFlowGroupId, expense.CashFlowId);
                if (expenseAddressLookup.ContainsKey(key))
                {
                    expenseAddressLookup[key].Add(workSheet.Cells[rowIndex, 5].FullAddress);
                }
                else
                {
                    expenseAddressLookup[key] = new List<string> { workSheet.Cells[rowIndex, 5].FullAddress };
                }

                rowIndex++;
            }

            workSheet.Cells[1, 4].Value = "Suma"; // Suma, Realizacja, Pozostało
            workSheet.Cells[1, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            workSheet.Cells[1, 5].Formula = string.Format("SUBTOTAL(9,E{0}:E{1})", 2, rowIndex - 1);
            SetBackgroundColor(workSheet.Cells[1, 5], Color.LightGreen);

            workSheet.Cells[rowIndex, 4].Value = "Suma"; // Suma, Realizacja, Pozostało
            workSheet.Cells[rowIndex, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            workSheet.Cells[rowIndex, 5].Formula = string.Format("SUBTOTAL(9,E{0}:E{1})", 2, rowIndex - 1);
            SetBackgroundColor(workSheet.Cells[rowIndex, 5], Color.LightGreen);
            workSheet.Row(1).Style.Font.Bold = true;
            workSheet.Row(rowIndex).Style.Font.Bold = true;

            FormatToDate(workSheet.Column(4));
            FormatToCurrency(workSheet.Column(5));

            AlterateAndAutoFitRowsColumns(workSheet, 3, expenses.Count() + 1, columnNames.Length);
            workSheet.View.FreezePanes(3, 1);

            return expenseAddressLookup;
        }

        private static string GetExpenseLookupKey(int budgetId, int cashFlowGroupId, int cashFlowId)
        {
            return string.Format("BudgetId:{0}|CashFlowGroupId:{1}|CashFlowId:{2}", budgetId, cashFlowGroupId, cashFlowId);
        }
        #endregion Expenses worksheet

        #region Not used for now...
        private static ExcelWorksheet CreateBudgetPlanWithExpensesWorksheet(IEnumerable<Budget> budgets, ExcelPackage excelPackage)
        {
            var columnNames = new string[]
                {
                    "Budżet",
                    "Grupa wydatków",
                    "Kategoria wydatków",
                    "Typ",
                    "Data",
                    "Kwota",
                    "Opis",
                };
            var workSheet = excelPackage.Workbook.Worksheets.Add("Plan i realizacja");
            CreateWorksheetHeader(workSheet, columnNames);
            List<int> expenseSummaryRows = new List<int>();
            List<int> planSummaryRows = new List<int>();
            int rowIndex = 2;
            foreach (var budget in budgets)
            {
                if (budget.BudgetPlanItems.Any())
                {
                    var plansGroupped = budget.BudgetPlanItems.OrderBy(x => x.CashFlow.Group.Position).GroupBy(x => x.CashFlow.GroupName);
                    foreach (var planGroup in plansGroupped)
                    {
                        var categories = planGroup.GroupBy(x => x.CashFlowId);
                        List<string> categoriesAddressList = new List<string>();
                        List<int> expenseSummaryRowsTmp = new List<int>();
                        foreach (var category in categories)
                        {
                            // fill all plan items for each category
                            using (var range = workSheet.Cells[rowIndex, 1].LoadFromArrays(category.Select(x => new object[] 
                            { 
                                string.Format("{0:yyyy-MM-dd} - {1:yyyy-MM-dd}", budget.DateFrom, budget.DateTo),
                                x.CashFlow.GroupName,
                                x.CashFlow.Name,
                                "Plan",
                                null,
                                x.Value,
                                x.Description
                            })))
                            {
                                categoriesAddressList.Add(string.Format("F{0}:F{1}", range.Start.Row, range.End.Row));
                            }

                            rowIndex += category.Count();

                            // insert expenses for this category
                            rowIndex = FillWithExpensesAndSummary(columnNames, workSheet, rowIndex, budget, category, expenseSummaryRowsTmp);
                        }
                        expenseSummaryRows.AddRange(expenseSummaryRowsTmp);
                        rowIndex = FillWithPlanSummary(workSheet, planSummaryRows, rowIndex, categoriesAddressList, expenseSummaryRowsTmp);
                    }
                }
            }

            FormatToDate(workSheet.Column(5));
            FormatToCurrency(workSheet.Column(6));
            AlterateAndAutoFitRowsColumns(workSheet, 2, rowIndex - 1, columnNames.Length);

            foreach (var row in expenseSummaryRows)
            {
                SetBackgroundColor(workSheet.Cells[row, 1, row, columnNames.Length], Color.White);
                SetBackgroundColor(workSheet.Cells[row, 4, row, 6], Color.LightGreen);
            }

            foreach (var row in planSummaryRows)
            {
                SetBackgroundColor(workSheet.Cells[row, 1, row, columnNames.Length], Color.White);
                SetBackgroundColor(workSheet.Cells[row, 4, row, 6], Color.LightBlue);
            }

            return workSheet;
        }

        private static int FillWithPlanSummary(ExcelWorksheet workSheet, List<int> summaryRows, int rowIndex, List<string> categoriesAddressList, List<int> expenseSummaryRows)
        {
            summaryRows.Add(rowIndex);
            workSheet.Cells[rowIndex, 1].Value = null;
            workSheet.Cells[rowIndex, 2].Value = null;
            workSheet.Cells[rowIndex, 3].Value = null;
            workSheet.Cells[rowIndex, 4].Value = "Plan łącznie";
            workSheet.Cells[rowIndex, 5].Value = null;
            workSheet.Cells[rowIndex, 6].Formula = string.Format("SUBTOTAL(9,{0})", string.Join(",", categoriesAddressList));
            workSheet.Cells[rowIndex, 7].Value = null;
            workSheet.Cells[rowIndex, 4, rowIndex, 6].Style.Font.Bold = true;
            rowIndex++;

            summaryRows.Add(rowIndex);
            workSheet.Cells[rowIndex, 1].Value = null;
            workSheet.Cells[rowIndex, 2].Value = null;
            workSheet.Cells[rowIndex, 3].Value = null;
            workSheet.Cells[rowIndex, 4].Value = "Pozostało do wydania";
            workSheet.Cells[rowIndex, 5].Value = null;
            var formula = string.Format("F{0} - SUM({1})", rowIndex - 1, string.Join(",", expenseSummaryRows.Select(x => string.Format("F{0}", x))));
            workSheet.Cells[rowIndex, 6].Formula = formula;
            workSheet.Cells[rowIndex, 7].Value = null;
            workSheet.Cells[rowIndex, 4, rowIndex, 6].Style.Font.Bold = true;
            rowIndex++;
            return rowIndex;
        }

        private static int FillWithExpensesAndSummary(string[] columnNames, ExcelWorksheet workSheet, int rowIndex, Budget budget, IGrouping<int, BudgetPlan> category, List<int> summaryRows)
        {
            var expenses = budget.Expenses.Where(x => x.CashFlowId == category.Key).ToList();
            if (expenses.Any())
            {
                workSheet.Cells[rowIndex, 1].LoadFromArrays(expenses.Select(x => new object[] 
                                    { 
                                        string.Format("{0:yyyy-MM-dd} - {1:yyyy-MM-dd}", budget.DateFrom, budget.DateTo),
                                        x.Flow.GroupName,
                                        x.Flow.Name,
                                        "Realizacja",
                                        x.Date,
                                        x.Value,
                                        x.Description
                                    }));

                rowIndex += expenses.Count;
            }

            summaryRows.Add(rowIndex);
            workSheet.Cells[rowIndex, 1].Value = null;
            workSheet.Cells[rowIndex, 2].Value = null;
            workSheet.Cells[rowIndex, 3].Value = null;
            workSheet.Cells[rowIndex, 4].Value = "Realizacja łącznie";
            workSheet.Cells[rowIndex, 5].Value = null;
            if (expenses.Any())
            {
                int subtotalFromRow = rowIndex - expenses.Count;
                int subtotalToRow = rowIndex - 1;
                workSheet.Cells[rowIndex, 6].Formula = string.Format("SUBTOTAL(9,F{0}:F{1})", subtotalFromRow, subtotalToRow);
            }
            else
            {
                workSheet.Cells[rowIndex, 6].Value = 0;
            }
            workSheet.Cells[rowIndex, 7].Value = null;
            workSheet.Cells[rowIndex, 1, rowIndex, columnNames.Length].Style.Font.Bold = true;
            rowIndex++;
            return rowIndex;
        }
        #endregion Not used for now...

        private static void CreatePivotTableForBudgetPlanWorksheet(ExcelPackage excelPackage, ExcelWorksheet budgetPlanWorksheet)
        {
            var pivotWorksheet = excelPackage.Workbook.Worksheets.Add("Plan pivot");
            using (var range = budgetPlanWorksheet.Cells[budgetPlanWorksheet.Dimension.Address])
            {
                var pivotTable = pivotWorksheet.PivotTables.Add(pivotWorksheet.Cells["A1"], range, "Plan pivot");

            }
        }

        private FileInfo CreateFile(string filename)
        {
            var file = new FileInfo(filename);
            if (file.Exists)
            {
                file.Delete();
                file = new FileInfo(filename);
            }
            return file;
        }

        private static void CreateWorksheetHeader(ExcelWorksheet workSheet, IList<string> headers, int startFromColumn = 1, int placeAtRow = 1)
        {
            int lastColumnDelta = startFromColumn - 1;
            for (int i = 1; i <= headers.Count; i++)
            {
                workSheet.Cells[placeAtRow, i + lastColumnDelta].Value = headers[i - 1];
            }

            using (var range = workSheet.Cells[placeAtRow, startFromColumn, placeAtRow, headers.Count + lastColumnDelta])
            {
                range.Style.Font.Bold = true;
                SetBackgroundColor(range, Color.Khaki);
            }
        }

        private static void AlterateAndAutoFitRowsColumns(ExcelWorksheet workSheet, int fromRow, int toRow, int numberOfColumns)
        {
            if (fromRow > toRow)
            {
                return;
            }
            for (int i = 1; i <= toRow - fromRow + 1; i++)
            {
                if (i % 2 == 0)
                {
                    int rowIndex = i + fromRow - 1;
                    SetBackgroundColor(workSheet.Cells[rowIndex, 1, rowIndex, numberOfColumns], Color.LightGoldenrodYellow);
                }
            }

            using (var range = workSheet.Cells[fromRow - 1, 1, toRow, numberOfColumns])
            {
                range.AutoFitColumns();
                range.AutoFilter = true;
            }
        }

        private static void SetBackgroundColor(ExcelRange range, Color color)
        {
            SetBackgroundColor(range.Style, color);
        }

        private static void SetBackgroundColor(ExcelColumn column, Color color)
        {
            SetBackgroundColor(column.Style, color);
        }

        private static void SetBackgroundColor(ExcelRow row, Color color)
        {
            SetBackgroundColor(row.Style, color);
        }

        private static void SetBackgroundColor(ExcelStyle style, Color color)
        {
            style.Fill.PatternType = ExcelFillStyle.Solid;
            style.Fill.BackgroundColor.SetColor(color);
        }

        private static void FormatToCurrency(ExcelRange range, bool negativeAsRed = true)
        {
            FormatToCurrency(range.Style);
        }

        private static void FormatToCurrency(ExcelColumn column, bool negativeAsRed = true)
        {
            FormatToCurrency(column.Style);
        }

        private static void FormatToCurrency(ExcelStyle style, bool negativeAsRed = true)
        {
            if (negativeAsRed)
            {
                style.Numberformat.Format = @"# ##0.00 zł;[Red]-# ##0.00 zł";
            }
            else
            {
                style.Numberformat.Format = @"#,##0.00zł";
            }
        }

        private static void FormatToDate(ExcelRange range)
        {
            FormatToDate(range.Style);
        }

        private static void FormatToDate(ExcelColumn column)
        {
            FormatToDate(column.Style);
        }
        
        private static void FormatToDate(ExcelStyle style)
        {
            style.Numberformat.Format = "yyyy-mm-dd";
        }

        #endregion Private methods
    }
}
