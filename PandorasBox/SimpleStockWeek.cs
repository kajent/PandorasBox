using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PandorasBox
{
    class SimpleStockWeek
    {
        private double _open;
        private double _high;
        private double _low;
        private double _close;
        private int _date;
        private UInt32 _volume = 0;
        private String _symbol = "No Symbol";

        public SimpleStockWeek(int date, UInt32 volume, double open, double high, double low, double close, String symbol)
        {
            _date = date;
            _volume = volume;
            _symbol = symbol;

            _open = open;
            _high = high;
            _low = low;
            _close = close;
        }

        #region Accessors
        public double getOpen()
        {
            return _open;
        }

        public double getHigh()
        {
            return _high;
        }

        public double getLow()
        {
            return _low;
        }

        public double getClose()
        {
            return _close;
        }

        public int getDate()
        {
            return _date;
        }

        public UInt32 getVolume()
        {
            return _volume;
        }

        public String getSymbol()
        {
            return _symbol;
        }

        public void setDate(int value)
        {
            _date = value;
        }       
        
        public void setOpen(double value)
        {
            _open = value;
        }

        public void setHigh(double value)
        {
            _high = value;
        }

        public void setLow(double value)
        {
            _low = value;
        }

        public void setClose(double value)
        {
            _close = value;
        }
        #endregion

        public double getShadowHeight()
        {
            return Math.Abs(this._high - this._low);
        }

        public double getBodyHeight()
        {
            return Math.Abs(this._open - this.getClose());
        }

        public double getShadowHeightBelow()
        {
            if (getDirection() == 1)
                return Math.Abs(this._low - this.getOpen());
            else
                return Math.Abs(this._low - this.getClose());
        }

        public double getShadowHeightAbove()
        {
            if (getDirection() == 1)
                return Math.Abs(this._high - this.getClose());
            else
                return Math.Abs(this._high - this.getOpen());
        }

        public int getDirection()
        {
            if (this.getClose() - this.getOpen() > 0)
                return 1;
            if (this.getClose() - this.getOpen() < 0)
                return -1;
            if (this.getClose() - this.getOpen() == 0)
                return 0;
            Console.WriteLine("Error in this plot point!");
            return -5;
        }
    }
}
