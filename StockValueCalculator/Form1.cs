﻿using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using HtmlAgilityPack;

namespace StockValueCalculator
{
    public partial class Form1 : Form
    {
        //parse input parameters before calculation
        private decimal marketPrice;
        private decimal tradingTaxRate;
        private decimal profitPerShare;
        private decimal profitSharingRate;
        private short companyDuration;
        private decimal discountRate;
        private decimal normalGrowthRate;
        private decimal highSpeedGrowthRate;
        private short highSpeedGrwothDuration;
        private decimal profitSharingTaxRate;
        private short depressionFrequency;
        private decimal depressionLossRate;
        private short stockHeldDuration;

        private decimal assumedStockPriceGrowth = new decimal(0.02);

        private string preferStockListFileName = "PreferStockList.csv";

        //temp variables for calculation
        private decimal totalInnerValue;
        private decimal currentInterest;
        private decimal totalProfitSharing;
        private decimal totalTradingTaxPaid;
        private decimal resultForSell;
        private decimal feePaid;
        private decimal totalBuyTax;
        private decimal totalSellTax;
        private decimal currentGrowth;

        private string successMessage = PromptMessages.successMessageEN;
        private string parseCompanyDetailsFormatError = PromptMessages.parseCompanyDetailsFormatErrorEN;
        private string parseCompanyDetailsSuccessMessage = PromptMessages.parseCompanyDetailsSuccessMessageEN;
        private string retrieveStockInfoSuccessMessage = PromptMessages.retrieveStockInfoSuccessMessageEN;
        private string retrieveStockInfoFailedMessage = PromptMessages.retrieveStockInfoFailedMessageEN;
        private string parseCompanyDetailsFromServerSuccessMessage = PromptMessages.parseCompanyDetailsFromServerSuccessMessageEN;
        private string parseCompanyDetailsFromServerError = PromptMessages.parseCompanyDetailsFromServerErrorEN;
        private string unknownStockIDMessage = PromptMessages.unknownStockIDMessageEN;
        private string clearPreferStockListSuccessMessage = PromptMessages.clearPreferStockListSuccessMessageEN;

        public Form1()
        {
            InitializeComponent();
            setLanguagePreference();
            setCalculationInitValues();
            setStockInfoInitValues();
        }

        #region "language preference"
        private void setLanguagePreference()
        {
            CultureInfo info = Thread.CurrentThread.CurrentUICulture;
            if (info.Name.Equals("zh-cn", StringComparison.CurrentCultureIgnoreCase))
            {
                lblMarketPrice.Text = ConfigurationManager.AppSettings.Get("lblMarketPrice_zh");
                lblTradeTaxRate.Text = ConfigurationManager.AppSettings.Get("lblTradingTaxRate_zh");
                lblProfitPerShare.Text = ConfigurationManager.AppSettings.Get("lblProfitPerShare_zh");
                lblProfitSharingRate.Text = ConfigurationManager.AppSettings.Get("lblProfitSharingRate_zh");
                lblCompanyDuration.Text = ConfigurationManager.AppSettings.Get("lblCompanyDuration_zh");
                lblDiscountRate.Text = ConfigurationManager.AppSettings.Get("lblDiscountRate_zh");
                lblNormalGrowthRate.Text = ConfigurationManager.AppSettings.Get("lblNormalGrowthRate_zh");
                lblHighSpeedGrowthRate.Text = ConfigurationManager.AppSettings.Get("lblHighSpeedGrowthRate_zh");
                lblHighSpeedGrowthDuration.Text = ConfigurationManager.AppSettings.Get("lblHighSpeedGrowthDuration_zh");
                lblProfitSharingTax.Text = ConfigurationManager.AppSettings.Get("lblProfitSharingTaxRate_zh");
                lblDepressionFrequency.Text = ConfigurationManager.AppSettings.Get("lblDepressionFrequency_zh");
                lblDepressionLossRate.Text = ConfigurationManager.AppSettings.Get("lblDepressionLossRate_zh");
                lblStockHeldDuration.Text = ConfigurationManager.AppSettings.Get("lblStockHeldDuration_zh");
                btnParseCompanyDetails.Text = ConfigurationManager.AppSettings.Get("btnParseCompanyDetails_zh");
                btnParseCompanyDetailsFromServer.Text = ConfigurationManager.AppSettings.Get("btnParseCompanyDetailsFromServer_zh");
                btnCalculate.Text = ConfigurationManager.AppSettings.Get("btnCalculate_zh");
                btnRetrieveStockInfo.Text = ConfigurationManager.AppSettings.Get("btnRefreshStockList_zh");
                btnClearPreferStockList.Text = ConfigurationManager.AppSettings.Get("btnClearPreferStockList_zh");

                lblSelectedStockID.Text = ConfigurationManager.AppSettings.Get("lblStockID_zh");
                lblCompanyName.Text = ConfigurationManager.AppSettings.Get("lblCompanyName_zh");
                lblLastTradingPrice.Text = ConfigurationManager.AppSettings.Get("lblLastTradingPrice_zh");
                lblCompanyProfitPerShare.Text = ConfigurationManager.AppSettings.Get("lblCompanyProfitPerShare_zh");
                lblDateOfInfo.Text = ConfigurationManager.AppSettings.Get("lblDateOfInfo_zh");
                lblPERatio.Text = ConfigurationManager.AppSettings.Get("lblPERatio_zh");
                lblFirstYearProfitSharing.Text = ConfigurationManager.AppSettings.Get("lblFirstYearProfitSharing_zh");
                lblSecondYearProfitSharing.Text = ConfigurationManager.AppSettings.Get("lblSecondYearProfitSharing_zh");
                lblThirdYearProfitSharing.Text = ConfigurationManager.AppSettings.Get("lblThirdYearProfitSharing_zh");
                lblFourthYearProfitSharing.Text = ConfigurationManager.AppSettings.Get("lblFourthYearProfitSharing_zh");
                lblFifthYearProfitSharing.Text = ConfigurationManager.AppSettings.Get("lblFifthYearProfitSharing_zh");

                calculationPage.Text = ConfigurationManager.AppSettings.Get("tabCalculationPage_zh");
                stockMarketPage.Text = ConfigurationManager.AppSettings.Get("tabStockMarketPage_zh");
                groupBoxForStockInfo.Text = ConfigurationManager.AppSettings.Get("groupStockInfo_zh");
                groupBoxForCheckingParameters.Text = ConfigurationManager.AppSettings.Get("groupBoxCheckingParameters_zh");
                groupBoxForServerParseParams.Text = ConfigurationManager.AppSettings.Get("groupBoxForServerParseParams_zh");
                groupBoxForManualInputParams.Text = ConfigurationManager.AppSettings.Get("groupBoxForManualInputParams_zh");

                checkBoxKeepPreferStockID.Text = ConfigurationManager.AppSettings.Get("checkBoxKeepPreferStockID_zh");

                successMessage = PromptMessages.successMessageZH;
                parseCompanyDetailsFormatError = PromptMessages.parseCompanyDetailsFormatErrorZH;
                parseCompanyDetailsSuccessMessage = PromptMessages.parseCompanyDetailsSuccessMessageZH;
                retrieveStockInfoSuccessMessage = PromptMessages.retrieveStockInfoSuccessMessageZH;
                retrieveStockInfoFailedMessage = PromptMessages.retrieveStockInfoFailedMessageZH;
                parseCompanyDetailsFromServerSuccessMessage = PromptMessages.parseCompanyDetailsFromServerSuccessMessageZH;
                parseCompanyDetailsFromServerError = PromptMessages.parseCompanyDetailsFromServerErrorZH;
                unknownStockIDMessage = PromptMessages.unknownStockIDMessageZH;
                clearPreferStockListSuccessMessage = PromptMessages.clearPreferStockListSuccessMessageZH;
            }
        }
        #endregion

        #region "set init values"
        private void setCalculationInitValues()
        {
            txtMarketPrice.Text = ConfigurationManager.AppSettings.Get("initValueMarketPrice");
            txtTradeTaxRate.Text = ConfigurationManager.AppSettings.Get("initValueTradingTaxRate");
            txtProfitPerShare.Text = ConfigurationManager.AppSettings.Get("initValueProfitPerShare");
            txtProfitSharingRate.Text = ConfigurationManager.AppSettings.Get("initValueProfitSharingRate");
            txtCompanyDuration.Text = ConfigurationManager.AppSettings.Get("initValueCompanyDuration");
            txtDiscountRate.Text = ConfigurationManager.AppSettings.Get("initValueDiscountRate");
            txtNormalGrowthRate.Text = ConfigurationManager.AppSettings.Get("initValueNormalGrowthRate");
            txtHighSpeedGrowthRate.Text = ConfigurationManager.AppSettings.Get("initValueHighSpeedGrowthRate");
            txtHighSpeedGrowthDuration.Text = ConfigurationManager.AppSettings.Get("initValueHighSpeedGrowthDuration");
            txtProfitSharingTax.Text = ConfigurationManager.AppSettings.Get("initValueProfitSharingTaxRate");
            txtDepressionFrequency.Text = ConfigurationManager.AppSettings.Get("initValueDepressionFrequency");
            txtDepressionLossRate.Text = ConfigurationManager.AppSettings.Get("initValueDepressionLossRate");
            txtStockHeldDuration.Text = ConfigurationManager.AppSettings.Get("initValueStockHeldDuration");
        }

        private void setStockInfoInitValues()
        {
            checkBoxKeepPreferStockID.Checked = Convert.ToBoolean(ConfigurationManager.AppSettings.Get("initValueKeepPreferStockList"));
            comboBoxStockIDList.Items.AddRange(Utils.readPreferStockIDList());
        }
        #endregion

        private void btnCalculate_Click(object sender, EventArgs e)
        {
            //Retrieve input params.
            parseInputParameters();

            //Declare temp variables for calculation
            resetTempCalVariables();

            for (var idx = 1; idx <= companyDuration; idx++) {
                //calculate the risk interest rate for current year
                currentInterest = currentInterest * (decimal.One / (decimal.One + discountRate));

                //If HSG is larger than 0, and current year has not reach the upper limit for HSG, then calculate for HSG
                if (highSpeedGrowthRate > 0 && highSpeedGrwothDuration >= idx) {
                    currentGrowth = currentGrowth * (decimal.One + highSpeedGrowthRate);
                } else {
                    currentGrowth = currentGrowth * (decimal.One + normalGrowthRate);
                }

                if (idx % depressionFrequency > 0)
                {
                    totalProfitSharing = totalProfitSharing + currentInterest * profitPerShare * currentGrowth * profitSharingRate * (decimal.One - profitSharingTaxRate);
                    totalTradingTaxPaid = totalTradingTaxPaid + currentInterest * profitPerShare * currentGrowth * profitSharingRate * profitSharingTaxRate;
                    totalInnerValue = totalInnerValue + currentInterest * profitPerShare * currentGrowth * (1 - profitSharingRate);
                    resultForSell = resultForSell + profitPerShare * currentGrowth * (1 - profitSharingRate);
                } else {
                    totalProfitSharing = totalProfitSharing + currentInterest * profitPerShare * currentGrowth * profitSharingRate * (decimal.One - profitSharingTaxRate) * (decimal.One - depressionLossRate);
                    totalTradingTaxPaid = totalTradingTaxPaid + currentInterest * profitPerShare * currentGrowth * profitSharingRate * profitSharingTaxRate * (decimal.One - depressionLossRate);
                    totalInnerValue = totalInnerValue + currentInterest * profitPerShare * currentGrowth * (decimal.One - profitSharingRate) * (decimal.One - depressionLossRate);
                    resultForSell = resultForSell + profitPerShare * currentGrowth * (decimal.One - profitSharingRate) * (decimal.One - depressionLossRate);
                }

                if (stockHeldDuration > 0 && idx % stockHeldDuration == 0) {
                    totalBuyTax = totalBuyTax + marketPrice * (decimal.One + idx * assumedStockPriceGrowth) * tradingTaxRate * currentInterest;
                    totalSellTax = totalSellTax + marketPrice * (decimal.One + idx * assumedStockPriceGrowth) * tradingTaxRate * currentInterest;
                }
            }
            totalSellTax = totalSellTax + resultForSell * currentInterest * tradingTaxRate;

            string displaySuccessMessage = successMessage
                                                .Replace("[_MARKET_PRICE_]", Convert.ToString(decimal.Round(marketPrice, 5)))
                                                .Replace("[_INNER_VALUE_DEDUCT_PROFIT_SHARING_]", Convert.ToString(decimal.Round(totalInnerValue, 5)))
                                                .Replace("[_PROFIT_SHARING_]", Convert.ToString(decimal.Round(totalProfitSharing, 5)))
                                                .Replace("[_TRADING_TAX_PAID_]", Convert.ToString(decimal.Round(totalTradingTaxPaid, 5)))
                                                .Replace("[_BUY_TAX_PAID_]", Convert.ToString(decimal.Round(totalBuyTax, 5)))
                                                .Replace("[_SELL_TAX_PAID_]",Convert.ToString(decimal.Round(totalSellTax, 5)))
                                                .Replace("[_INNER_VALUE_]", Convert.ToString(decimal.Round(totalInnerValue + totalProfitSharing - totalBuyTax - totalSellTax - totalTradingTaxPaid, 5)))
                                                .Replace("[_MARKET_PRICE_TO_INNER_VALUE_]", Convert.ToString(decimal.Round(marketPrice / (totalInnerValue + totalProfitSharing - totalBuyTax - totalSellTax - totalTradingTaxPaid) * 100, 5)));

            MessageBox.Show(displaySuccessMessage, "Calculation Result");
        }

        #region "Reset variables"
        private void resetTempCalVariables()
        {
            totalInnerValue = decimal.Zero;
            currentInterest = decimal.One;
            totalProfitSharing = decimal.Zero;
            totalTradingTaxPaid = decimal.Zero;
            resultForSell = decimal.Zero;
            feePaid = decimal.Zero;
            totalBuyTax = marketPrice * tradingTaxRate;
            totalSellTax = decimal.Zero;
            currentGrowth = decimal.One;
        }

        private void resetStockInfoPageParameters()
        {
            txtCompanyName.Text = "";
            txtDateOfInfo.Text = "";
            txtLastTradingPrice.Text = "";
            txtCompanyProfitPerShare.Text = "";
            txtPERatio.Text = "";
            txtFirstYearProfitSharing.Text = "";
            txtSecondYearProfitSharing.Text = "";
            txtThirdYearProfitSharing.Text = "";
            txtFourthYearProfitSharing.Text = "";
            txtFifthYearProfitSharing.Text = "";
        }
        #endregion

        private void parseInputParameters()
        {
            decimal.TryParse(txtMarketPrice.Text.Trim(), out marketPrice);
            decimal.TryParse(txtTradeTaxRate.Text.Trim(), out tradingTaxRate);
            tradingTaxRate = tradingTaxRate / 100M;
            decimal.TryParse(txtProfitPerShare.Text.Trim(), out profitPerShare);
            decimal.TryParse(txtProfitSharingRate.Text.Trim(), out profitSharingRate);
            profitSharingRate = profitSharingRate / 100M;
            short.TryParse(txtCompanyDuration.Text.Trim(), out companyDuration);
            decimal.TryParse(txtDiscountRate.Text.Trim(), out discountRate);
            discountRate = discountRate / 100M;
            decimal.TryParse(txtNormalGrowthRate.Text.Trim(), out normalGrowthRate);
            normalGrowthRate = normalGrowthRate / 100M;
            decimal.TryParse(txtHighSpeedGrowthRate.Text.Trim(), out highSpeedGrowthRate);
            highSpeedGrowthRate = highSpeedGrowthRate / 100M;
            short.TryParse(txtHighSpeedGrowthDuration.Text.Trim(), out highSpeedGrwothDuration);
            decimal.TryParse(txtProfitSharingTax.Text.Trim(), out profitSharingTaxRate);
            profitSharingTaxRate = profitSharingTaxRate / 100M;
            short.TryParse(txtDepressionFrequency.Text.Trim(), out depressionFrequency);
            decimal.TryParse(txtDepressionLossRate.Text.Trim(), out depressionLossRate);
            depressionLossRate = depressionLossRate / 100M;
            short.TryParse(txtStockHeldDuration.Text.Trim(), out stockHeldDuration);
        }

        private void parseInputParametersFromFile(string line)
        {
            string[] splitLines = line.Trim().Split(',');
            if (splitLines.Length < 2) return;
            string fieldName = splitLines[0].Trim().ToLowerInvariant();
            string fieldValue = splitLines[1].Trim();

            Control.ControlCollection controlCollection = this.tabControl1.GetControl(0).Controls;
            CompanyFileInfo companyFileInfo = new CompanyFileInfo();

            // Form -> tabControl -> GroupBoxs -> TextBoxs
            foreach(Control control in this.tabControl1.GetControl(0).Controls)
            {
                if(control.GetType() == typeof(GroupBox))
                {
                    foreach(Control subControl in control.Controls)
                    {
                        if (companyFileInfo.getControlName(fieldName) == subControl.Name)
                        {
                            subControl.Text = fieldValue;
                        }
                    }
                }
            }
        }

        private void btnParseCompanyDetails_Click(object sender, EventArgs e)
        {
            openFileDialogForCompanyDetails.ShowDialog();
            string selectedFilePath = openFileDialogForCompanyDetails.FileName;

            this.parseCompanyDetailsFromFile(selectedFilePath);
        }

        private void parseCompanyDetailsFromFile(string filePath)
        {
            if (!filePath.EndsWith(".csv"))
            {
                MessageBox.Show(parseCompanyDetailsFormatError, "File Format Error");
                return;
            }

            foreach (string line in File.ReadAllLines(filePath))
            {
                parseInputParametersFromFile(line);
            }

            MessageBox.Show(parseCompanyDetailsSuccessMessage.Replace("[_FILE_PATH_]", filePath));
        }

        private void btnParseCompanyDetailsFromServer_Click(object sender, EventArgs e)
        {
            if (!txtCompanyName.Text.Trim().Equals(""))
            {
                txtMarketPrice.Text = txtLastTradingPrice.Text;
                txtProfitPerShare.Text = txtCompanyProfitPerShare.Text;

                decimal firstYearProfitSharing = decimal.Zero;
                decimal secondYearProfitSharing = decimal.Zero;
                decimal thirdYearProfitSharing = decimal.Zero;
                decimal fourthYearProfitSharing = decimal.Zero;
                decimal fifthYearProfitSharing = decimal.Zero;

                decimal.TryParse(txtFirstYearProfitSharing.Text, out firstYearProfitSharing);
                decimal.TryParse(txtSecondYearProfitSharing.Text, out secondYearProfitSharing);
                decimal.TryParse(txtThirdYearProfitSharing.Text, out thirdYearProfitSharing);
                decimal.TryParse(txtFourthYearProfitSharing.Text, out fourthYearProfitSharing);
                decimal.TryParse(txtFifthYearProfitSharing.Text, out fifthYearProfitSharing);

                decimal profitSharingPerShare = (firstYearProfitSharing + secondYearProfitSharing +
                                                    thirdYearProfitSharing + fourthYearProfitSharing +
                                                    fifthYearProfitSharing) / 50;
                if (profitSharingPerShare > decimal.Zero)
                {
                    decimal companyProfitPerShare = decimal.Zero;
                    decimal.TryParse(txtCompanyProfitPerShare.Text, out companyProfitPerShare);
                    txtProfitSharingRate.Text = Convert.ToString(decimal.Round(profitSharingPerShare / companyProfitPerShare, 4) * 100);
                }

                MessageBox.Show(parseCompanyDetailsFromServerSuccessMessage.Replace("[_COMPANY_NAME_]", txtCompanyName.Text.Trim()));

                tabControl1.SelectTab(calculationPage);
                resetStockInfoPageParameters();
            }
            else
            {
                MessageBox.Show(parseCompanyDetailsFromServerError);
            }
            
        }

        private void btnRetrieveStockInfo_Click(object sender, EventArgs e)
        {
            
            string stockID = comboBoxStockIDList.Text.Trim();

            StockMarketTypes marketType = Utils.checkMarketType(stockID);

            switch (marketType)
            {
                case StockMarketTypes.CHINA_SZ_EXCHANGE_MARKET:
                    retrieveStockInfoByID(stockID,"sz" + stockID);
                    break;
                case StockMarketTypes.CHINA_SH_EXCHANGE_MARKET:
                    retrieveStockInfoByID(stockID,"sh" + stockID);
                    break;
                case StockMarketTypes.HK_EXCHANGE_MARKET:
                    retrieveStockInfoByID(stockID,"hk" + stockID);
                    break;
                case StockMarketTypes.UNKNOWN:
                    MessageBox.Show(unknownStockIDMessage);
                    break;
            }                
            
        }

        private void retrieveStockInfoByID(string originalStockID, string stockID)
        {
            string stockInfoRequestURL = URLTemplates.baiduTemplateByID.Replace("[_STOCK_ID_]", stockID);
            string lastProfitSharingRequestURL = URLTemplates.ifengProfitSharingTemplateByID.Replace("[_STOCK_ID_]", originalStockID);

            StockInfo stockInfo = new StockInfo();

            Utils.parseCompanyBasicInfo(stockInfoRequestURL, ref stockInfo);
            Utils.parseCompanyProfitSharing(lastProfitSharingRequestURL, ref stockInfo);

            if(stockInfo.CompanyName != "")
            {
                txtCompanyName.Text = stockInfo.CompanyName;
                txtLastTradingPrice.Text = stockInfo.LastTradingPrice;
                txtDateOfInfo.Text = stockInfo.DateOfInfo;
                txtCompanyProfitPerShare.Text = stockInfo.CompanyProfitPerShare;
                txtPERatio.Text = stockInfo.PERatio;

                txtFirstYearProfitSharing.Text = Convert.ToString(stockInfo.getProfitSharingInLastYear(-1));
                txtSecondYearProfitSharing.Text = Convert.ToString(stockInfo.getProfitSharingInLastYear(-2));
                txtThirdYearProfitSharing.Text = Convert.ToString(stockInfo.getProfitSharingInLastYear(-3));
                txtFourthYearProfitSharing.Text = Convert.ToString(stockInfo.getProfitSharingInLastYear(-4));
                txtFifthYearProfitSharing.Text = Convert.ToString(stockInfo.getProfitSharingInLastYear(-5));

                MessageBox.Show(retrieveStockInfoSuccessMessage);

                // Persist new company info after successfully retrieved
                if (checkBoxKeepPreferStockID.Checked)
                {
                    string persistString = "\n" + originalStockID + ","
                                                + txtCompanyName.Text + ","
                                                + txtDateOfInfo.Text + ","
                                                + txtLastTradingPrice.Text + ","
                                                + txtCompanyProfitPerShare.Text + ","
                                                + txtPERatio.Text + ","
                                                + txtFirstYearProfitSharing.Text + ","
                                                + txtSecondYearProfitSharing.Text + ","
                                                + txtThirdYearProfitSharing.Text + ","
                                                + txtFourthYearProfitSharing.Text + ","
                                                + txtFifthYearProfitSharing.Text;
                    if (!File.Exists(preferStockListFileName) || !File.ReadAllText(preferStockListFileName).Contains(persistString))
                    {
                        File.AppendAllText(preferStockListFileName, persistString);
                    }                  
                    comboBoxStockIDList.Items.Clear();
                    comboBoxStockIDList.Items.AddRange(Utils.readPreferStockIDList());
                }
            } else
            {
                MessageBox.Show(retrieveStockInfoFailedMessage);
            }
        }

        private void tabControl1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                if(tabControl1.SelectedTab == calculationPage)
                {
                    string[] fileNames = (string[])e.Data.GetData(DataFormats.FileDrop, false);
                    foreach (string fileName in fileNames)
                    {
                        this.parseCompanyDetailsFromFile(fileName);
                    }
                }
                else if(tabControl1.SelectedTab == stockMarketPage)
                {
                    //place holder for implementation of the drag and drop operation in stock market page
                }
            }
        }

        private void tabControl1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.All;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void btnClearPreferStockList_Click(object sender, EventArgs e)
        {
            if (File.Exists(preferStockListFileName))
            {
                File.WriteAllText(preferStockListFileName, "");
            }

            comboBoxStockIDList.Items.Clear();

            MessageBox.Show(clearPreferStockListSuccessMessage);
        }

        private void comboBoxStockIDList_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idx = comboBoxStockIDList.SelectedIndex;
            string[] fileContent = File.ReadAllLines(preferStockListFileName);
            string[] fileContentArray = fileContent[idx].Split(',');

            if (fileContent.Length >= idx)
            {
                txtCompanyName.Text = Utils.getValueFromArray(fileContentArray, 1);
                txtDateOfInfo.Text = Utils.getValueFromArray(fileContentArray, 2);
                txtLastTradingPrice.Text = Utils.getValueFromArray(fileContentArray, 3);
                txtCompanyProfitPerShare.Text = Utils.getValueFromArray(fileContentArray, 4);
                txtPERatio.Text = Utils.getValueFromArray(fileContentArray, 5);
                txtFirstYearProfitSharing.Text = Utils.getValueFromArray(fileContentArray, 6);
                txtSecondYearProfitSharing.Text = Utils.getValueFromArray(fileContentArray, 7);
                txtThirdYearProfitSharing.Text = Utils.getValueFromArray(fileContentArray, 8);
                txtFourthYearProfitSharing.Text = Utils.getValueFromArray(fileContentArray, 9);
                txtFifthYearProfitSharing.Text = Utils.getValueFromArray(fileContentArray, 10);
            }
        }
    }
}