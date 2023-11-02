using LiveCharts.Defaults;
using LiveCharts;
using LiveCharts.Wpf;

namespace Doctor
{
    public class StepLineSeriesBuilder
    {
        private StepLineSeries stepSerie;

        public StepLineSeriesBuilder(string stepSerieTitle)
        {
            stepSerie = new StepLineSeries
            {
                Title = stepSerieTitle,
                Values = new ChartValues<ObservablePoint>(),
            };
        }

        public StepLineSeries GetStepLineSerie()
        {
            return stepSerie; 
        }
    }
}
