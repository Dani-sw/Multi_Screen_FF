using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Multi_Screen_App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //NOOOOO il problema è con il mio PC w10 che lascia applicazioni in background ed anche quando si chiudono, per via di alcumi programmi che devo ancora scoprire
        //si sospetta nvidia driver o GPU

        public App()
        {//altrimenti crasha sileziosamentee rimane in memoria durante il dimensionamento a causa della GPU e dwpf con troppi effetti da ricalcolare...mah(dropeffect etc...fonte Claude)
           //RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;
        }
    }
}
