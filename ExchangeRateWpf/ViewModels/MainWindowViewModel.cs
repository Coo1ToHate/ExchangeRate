using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml.Linq;
using ExchangeRateWpf.Models;

namespace ExchangeRateWpf.ViewModels
{
    class MainWindowViewModel : DefaultViewModel
    {
        private WebClient _client;
        private ObservableCollection<Currency> _currencyList;
        private Currency _selectedCurrency;
        private DateTime _selectedDate;
        private Chart _chart;

        public MainWindowViewModel(Chart chart)
        {
            _client = new WebClient();
            CurrencyList = LoadCurrencyList();
            Chart = chart;

            #region Подготовка к построению графика

            Chart.ChartAreas.Add(new ChartArea("Def"));
            Chart.Series.Add(new Series("ser1"));
            Chart.Series["ser1"].ChartArea = "Def";
            Chart.Series["ser1"].ChartType = SeriesChartType.Line;
            string info = "Value: " + "#VAL{N2}" + "\n" + "Date: " + "#VALX" + "\n" + "Minimum: " + "#MIN{N2}" + "\n" + "Maximum: " + "#MAX{N2}" + "\n"
                          + "First: " + "#FIRST{N2}" + "\n" + "Last: " + "#LAST{N2}";
            Chart.Series["ser1"].ToolTip = info;

            #endregion

            SelectedCurrency = CurrencyList.First(c => c.ParentCode.Contains("R01235"));
            SelectedDate = DateTime.Now.AddYears(-1);
        }

        public ObservableCollection<Currency> CurrencyList
        {
            get => _currencyList;
            set
            {
                _currencyList = value;
                OnPropertyChanged();
            }
        }

        public Currency SelectedCurrency
        {
            get => _selectedCurrency;
            set
            {
                _selectedCurrency = value;
                ChangeRate();
                OnPropertyChanged();
            }
        }

        public Chart Chart
        {
            get => _chart;
            set
            {
                _chart = value;
                OnPropertyChanged();
            }
        }

        private void ChangeRate()
        {
            string dateNow = DateTime.Now.ToString("d", CultureInfo.CreateSpecificCulture("fr-FR"));
            string dateFrom = SelectedDate.ToString().Split(' ')[0].Replace('.', '/');
            string url = @$"http://www.cbr.ru/scripts/XML_dynamic.asp?date_req1={dateFrom}&date_req2={dateNow}&VAL_NM_RQ={SelectedCurrency.ParentCode}";
            var xml = _client.DownloadString(url);
            XDocument document = XDocument.Parse(xml);
            var elements = document.Element("ValCurs").Elements("Record");
            List<string> axisXData = new List<string>();
            List<double> axisYData = new List<double>();
            foreach (var e in elements)
            {
                axisXData.Add(e.Attribute("Date").Value);
                axisYData.Add(double.Parse(e.Element("Value").Value));
            }

            if (Chart.Series[0].Points.Count > 0)
            {
                Chart.Series[0].Points.Clear();
            }
            Chart.Series["ser1"].Points.DataBindXY(axisXData, axisYData);
            Chart.DataBind();
        }

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;
                ChangeRate();
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Currency> LoadCurrencyList()
        {
            string url = @"http://www.cbr.ru/scripts/XML_val.asp?d=0";
            var xml = _client.DownloadString(url);
            XDocument document = XDocument.Parse(xml);
            var elements = document.Element("Valuta").Elements("Item");

            ObservableCollection<Currency> list = new ObservableCollection<Currency>();

            foreach (var e in elements)
            {
                list.Add(new Currency
                {
                    Name = e.Element("Name")?.Value,
                    Nominal = int.Parse(e.Element("Nominal").Value),
                    ParentCode = e.Element("ParentCode")?.Value
                });
            }

            return list;
        }
    }
}
