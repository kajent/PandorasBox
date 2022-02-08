using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PandorasBox
{
    class SimpleStockPoint
    {
        private int _dateMod = 0;
        private int _date = 0;
        private UInt32 _volume = 0;
        private double _close = 0;
        private String _symbol = "No Symbol";

        public SimpleStockPoint(int date, UInt32 volume, double closingPrice, String symbol)
        {
            _dateMod = date;
            _date = date;
            _volume = volume;
            _close = closingPrice;
            _symbol = symbol;
        }

        public int getDate()
        {
            return _date;
        }

        public int getDateMod()
        {
            return _dateMod;
        }

        public void setDateMod(int value)
        {
            _dateMod = value;
        }

        public UInt32 getVolume()
        {
            return _volume;
        }

        public double getClose()
        {
            return _close;
        }

        public void setClose(double value)
        {
            _close = value;
        }

        public String getSymbol()
        {
            return _symbol;
        }
    }
}
