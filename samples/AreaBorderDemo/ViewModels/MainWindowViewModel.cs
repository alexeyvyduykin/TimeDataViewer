using TimeDataViewer.Core;

namespace AreaBorderDemo.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly PlotModel _area;

        public MainWindowViewModel()
        {

            //CoreFactory factory = new CoreFactory();

            //_area = factory.CreateArea();

            //_area.UpdateSize(400, 160);
        }

        public PlotModel Area => _area;

    }
}
