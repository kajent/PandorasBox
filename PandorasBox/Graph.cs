using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace PandorasBox
{
    public partial class Graph : Form
    {
        public Graph()
        {
            InitializeComponent();
        }

        public Chart getGraph()
        {
            return MarketGraph;
        }

    }
}
