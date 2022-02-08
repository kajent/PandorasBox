using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms.DataVisualization.Charting;
using System.Data;

namespace PandorasBox
{
    class TradeSettings
    {
        public Utilities.BandZones BollingerBandRange;
        public Utilities.ForceType ForceType;
        public Utilities.RSIType RSIType;
        public Utilities.MACD_around_Signal MACD_Around_Signal;
        public Utilities.MACD_around_Zero MACD_Around_Zero;
        public Utilities.HeikenAshiType HeikenAshiType;
        public int HeikenAshiStrength;
        public Utilities.StochasticType StochasticType;
        public Utilities.TrendInterval TopInterval;
        public Utilities.TrendInterval BottomInterval;
        public Utilities.ADXTrend ADXTrend;
        public Utilities.ADX_PDIvsNDI ADX_PDIvsNDI;
        public Utilities.ADX_Trending ADXTrending;
                
        public Utilities.Trend MA_SpecifiedTrend;
        public Utilities.TrendInterval MA_specifiedInterval;
        public Double MA_maxStrength;
        public Double MA_minStrength;
        public Boolean MA_exponential;
        public Utilities.MA_around_MA MA_QtoS;
        public Utilities.MA_around_MA MA_StoI;
        public Utilities.MA_around_MA MA_ItoL;
        public Utilities.Stock_around_MA MA_StocktoMA;
             

        public Utilities.TradeType TradeType;
        
        bool dojiStar;
        bool dojiDragonFly;
        bool dojiGraveStone;
        bool dojiPierce;
        bool dojiBearishEngulf;
        bool dojiBullishEngulf;
        bool dojiDarkCloud;

        public Boolean FilterBy_Bollinger = false;
        public Boolean FilterBy_Force = false;
        public Boolean FilterBy_RSI = false;
        public Boolean FilterBy_MACD = false;
        public Boolean FilterBy_Stochastic = false;
        public Boolean FilterBy_MA = false;
        public Boolean FilterBy_HeikenAshi = false;
        public Boolean FilterBy_Doji = false;
        public Boolean Filterby_ADX = false;
        
        public Func<Double[], Double[], Double, Boolean> MeetsBollingerRequirement_Buy;
        public Func<Double, Boolean> MeetsForceRequirements_Buy;
        public Func<HeikenAshiStockPoint, Utilities.HeikenAshiType, Boolean> MeetsHeikenAshiRequirement_Buy;
        public Func<Double, Double, Boolean> MeetsMACD_AroundSignal_Requirements_Buy;
        public Func<Double, Double, Boolean> MeetsMACD_AroundZero_Requirements_Buy;
        public Func<Stock, int, int, Boolean> MeetsStochasticRequirements_Buy;
        public Func<Stock, int, Boolean> MeetsRSIRequirements_Buy;
        public Func<Stock, int, Boolean> MeetsDojiRequirements_Buy;
        public Func<Double, Boolean> MeetsADXRequirements_Buy;
        public Func<Double, Double, Boolean> MeetsDIRequirements_Buy;
        public Func<Series, int, Boolean> MeetsADXTrendingRequirements_Buy;
        public Func<Stock, int, Boolean> MeetsMATrendStrengthRequirements_Buy;
        public Func<Stock, int, Boolean> MeetsMA_XtoY_Buy;
        public Func<Stock, int, Boolean> MeetsStockvsMA_Buy;

        public Func<Double[], Double[], Double, Boolean> MeetsBollingerRequirement_Sell;
        public Func<Double, Boolean> MeetsForceRequirements_Sell;
        public Func<HeikenAshiStockPoint, Utilities.HeikenAshiType, Boolean> MeetsHeikenAshiRequirement_Sell;
        public Func<Double, Double, Boolean> MeetsMACD_AroundSignal_Requirements_Sell;
        public Func<Double, Double, Boolean> MeetsMACD_AroundZero_Requirements_Sell;
        public Func<Stock, int, int, Boolean> MeetsStochasticRequirements_Sell;
        public Func<Stock, int, Boolean> MeetsRSIRequirements_Sell;
        public Func<Stock, int, Boolean> MeetsDojiRequirements_Sell;
        public Func<Double, Boolean> MeetsADXRequirements_Sell;
        public Func<Double, Double, Boolean> MeetsDIRequirements_Sell;
        public Func<Series, int, Boolean> MeetsADXTrendingRequirements_Sell;
        public Func<Stock, int, Boolean> MeetsMATrendStrengthRequirements_Sell;
        public Func<Stock, int, Boolean> MeetsMA_XtoY_Sell;
        public Func<Stock, int, Boolean> MeetsStockvsMA_Sell;

            
        public TradeSettings(Utilities.TradeType Trade, Object[] FormElements)
        {
            BollingerBandRange = (Utilities.BandZones)FormElements[0];
            ForceType = (Utilities.ForceType)FormElements[1];
            RSIType = (Utilities.RSIType)FormElements[2];
            StochasticType = (Utilities.StochasticType)FormElements[3];
            MACD_Around_Signal = (Utilities.MACD_around_Signal)FormElements[4];
            MACD_Around_Zero = (Utilities.MACD_around_Zero)FormElements[5];
            HeikenAshiType = (Utilities.HeikenAshiType)FormElements[6];

            ADXTrend = (Utilities.ADXTrend)FormElements[7];
            ADX_PDIvsNDI = (Utilities.ADX_PDIvsNDI)FormElements[8];
            ADXTrending = (Utilities.ADX_Trending)FormElements[10];

            List<Object> MA_Conditions = FormElements[11] as List<Object>;

            MA_SpecifiedTrend = (Utilities.Trend)MA_Conditions[0];
            MA_specifiedInterval = (Utilities.TrendInterval)MA_Conditions[1];
            MA_maxStrength = (Double)MA_Conditions[2];
            MA_minStrength = (Double)MA_Conditions[3];
            MA_QtoS = (Utilities.MA_around_MA)MA_Conditions[4];
            MA_StoI = (Utilities.MA_around_MA)MA_Conditions[5];
            MA_ItoL = (Utilities.MA_around_MA)MA_Conditions[6];
            MA_StocktoMA = (Utilities.Stock_around_MA)MA_Conditions[7];

            
           
            
            dojiStar = (FormElements[9] as Boolean[])[0];
            dojiDragonFly = (FormElements[9] as Boolean[])[1];
            dojiGraveStone = (FormElements[9] as Boolean[])[2];
            dojiPierce = (FormElements[9] as Boolean[])[3];
            dojiBearishEngulf = (FormElements[9] as Boolean[])[4];
            dojiBullishEngulf = (FormElements[9] as Boolean[])[5];
            dojiDarkCloud = (FormElements[9] as Boolean[])[6];
            HeikenAshiStrength = ((Int32)FormElements[12]);
            TradeType = Trade;

            CrunchComparisonConditions();
        }


        public Boolean SomeConditionSet()
        {
            return FilterBy_Bollinger || FilterBy_Force || FilterBy_RSI || FilterBy_MACD
                || FilterBy_Stochastic || FilterBy_MA || FilterBy_HeikenAshi || FilterBy_Doji || Filterby_ADX;
        }

        public void CrunchComparisonConditions(){
            if (TradeType == Utilities.TradeType.Buy)
            {

                switch (BollingerBandRange)
                {
                    case Utilities.BandZones.UpperBand:
                        MeetsBollingerRequirement_Buy = (PriceHighsAndLows, BandTopAndBottom, MAValue) =>
                        { return PriceHighsAndLows[0] > BandTopAndBottom[0]; };
                        break;

                    case Utilities.BandZones.LowerBand:
                        MeetsBollingerRequirement_Buy = (PriceHighsAndLows, BandTopAndBottom, MAValue) =>
                        { return PriceHighsAndLows[1] < BandTopAndBottom[1]; };
                        break;

                    //Price_Low < MA_Value && Price_High > MA_Value
                    case Utilities.BandZones.Middle:
                        MeetsBollingerRequirement_Buy = (PriceHighsAndLows, BandTopAndBottom, MAValue) =>
                        { return PriceHighsAndLows[0] > MAValue && PriceHighsAndLows[1] < MAValue; };
                        break;
                    default:
                        throw new Exception("Bollinger Error!");
                        break;
                }

                switch (ForceType)
                {
                    case Utilities.ForceType.Bullish:
                        MeetsForceRequirements_Buy = (forceValue) =>
                            { return (forceValue > 0); };
                        break;
                    case Utilities.ForceType.Bearish:
                        MeetsForceRequirements_Buy = (forceValue) =>
                            { return (forceValue < 0); };
                        break;
                    default:
                        throw new Exception("Force Error");
                        break;

                }

                switch (HeikenAshiType)
                {
                    default:
                        MeetsHeikenAshiRequirement_Buy = (HeikenAshiStockPoint, DesiredHA) =>
                        { return HeikenAshiStockPoint.getHeikenAshiSignal() == DesiredHA; };
                        break;
                }

                switch (MACD_Around_Signal)
                {
                    case Utilities.MACD_around_Signal.Above:
                        MeetsMACD_AroundSignal_Requirements_Buy = (MacdValue, MacdSignalValue) =>
                            { return MacdValue > MacdSignalValue; };
                        break;
                    case Utilities.MACD_around_Signal.Below:
                        MeetsMACD_AroundSignal_Requirements_Buy = (MacdValue, MacdSignalValue) =>
                            { return MacdValue < MacdSignalValue; };
                        break;
                    case Utilities.MACD_around_Signal.Irrelevant:
                        MeetsMACD_AroundSignal_Requirements_Buy = (MacdValue, MacdSignalValue) =>
                            { return true; };
                        break;
                    default:
                        throw new Exception("MACD Around Signal Error");
                        break;
                }

                switch (MACD_Around_Zero)
                {
                    case Utilities.MACD_around_Zero.Above:
                        MeetsMACD_AroundZero_Requirements_Buy = (MacdValue, MacdSignalValue) =>
                        { return MacdValue > 0; };
                        break;
                    case Utilities.MACD_around_Zero.Below:
                        MeetsMACD_AroundZero_Requirements_Buy = (MacdValue, MacdSignalValue) =>
                        { return MacdValue < 0; };
                        break;
                    case Utilities.MACD_around_Zero.Irrelevant:
                        MeetsMACD_AroundZero_Requirements_Buy = (MacdValue, MacdSignalValue) =>
                        { return true; };
                        break;
                    default:
                        throw new Exception("MACD Around Zero Error");
                        break;
                }

                switch (StochasticType)
                {
                    case Utilities.StochasticType.Bullish:
                        MeetsStochasticRequirements_Buy = (SingleStock, StochIndex, StochSMAIndex) =>
                            { return SingleStock.Stochastic_Oversold(StochIndex, StochSMAIndex); };
                        break;
                    case Utilities.StochasticType.Bearish:
                        MeetsStochasticRequirements_Buy = (SingleStock, StochIndex, StochSMAIndex) =>
                            { return SingleStock.Stochastic_Overbought(StochIndex, StochSMAIndex); };
                        break;
                }

                switch (RSIType)
                {
                    case Utilities.RSIType.Bullish:
                        MeetsRSIRequirements_Buy = (SingleStock, RSIIndex) =>
                            { return SingleStock.RSI_OverSold(RSIIndex); };
                        break;
                    case Utilities.RSIType.Bearish:
                        MeetsRSIRequirements_Buy = (SingleStock, RSIIndex) =>
                            { return SingleStock.RSI_OverBought(RSIIndex); };
                        break;
                }

                switch(ADXTrend)
                {
                    case Utilities.ADXTrend.NoTrend:
                        MeetsADXRequirements_Buy = (currentADX) =>
                            { return !(currentADX > Utilities.ADX_normal_trend_line); };
                        break;

                    case Utilities.ADXTrend.Normal:
                        MeetsADXRequirements_Buy = (currentADX) =>
                            { return (currentADX > Utilities.ADX_normal_trend_line); };
                        break;

                    case Utilities.ADXTrend.Strong:
                        MeetsADXRequirements_Buy = (currentADX) =>
                            { return (currentADX > Utilities.ADX_strong_trend_line);};
                        break;

                    case Utilities.ADXTrend.Extreme:
                        MeetsADXRequirements_Buy = (currentADX) =>
                            { return (currentADX > Utilities.ADX_extreme_trend_line); };
                        break;
                    case Utilities.ADXTrend.Irrelevant:
                        MeetsADXRequirements_Buy = (currentADX) =>
                            { return true; };
                        break;
                }

                switch (ADX_PDIvsNDI)
                {
                    case Utilities.ADX_PDIvsNDI.PDI_above_NDI:
                        MeetsDIRequirements_Buy = (currentPDI, currentNDI) =>
                            {return (currentPDI > currentNDI);};
                        break;
                    case Utilities.ADX_PDIvsNDI.NDI_above_PDI:
                        MeetsDIRequirements_Buy = (currentPDI, currentNDI) =>
                            { return (currentNDI > currentPDI); };
                        break;
                    case Utilities.ADX_PDIvsNDI.Irrelevant:
                        MeetsDIRequirements_Buy = (currentPDI, currentNDI) =>
                            { return true; };
                        break;
                }

                switch (ADXTrending)
                {
                    case Utilities.ADX_Trending.Trending:
                        MeetsADXTrendingRequirements_Buy = (ADX, Index) =>
                            {
                                int TrendStrength = 2;
                                for (int j = Index; j > Index - TrendStrength; j--)
                                {
                                    if (ADX.Points[j].YValues[0] < ADX.Points[j - 1].YValues[0])
                                        return false;
                                }
                                return true;
                            };
                        break;
                    case Utilities.ADX_Trending.NotTrending:
                        MeetsADXTrendingRequirements_Buy = (ADX, Index) =>
                        { return ADX.Points[Index].YValues[0] <= ADX.Points[Index - 1].YValues[0]; };
                        break;
                    case Utilities.ADX_Trending.Irrelevent:
                        MeetsADXTrendingRequirements_Buy = (SingleStock, Index) =>
                            { return true; };
                        break;
                }

                MeetsMATrendStrengthRequirements_Buy = (SelectedStock, Index) =>
                {
                    Boolean Buy = false;
                    if (MA_maxStrength <= MA_minStrength)
                        return true;

                    if (MA_SpecifiedTrend == Utilities.Trend.Irrelevant)
                        return true;
    

                    switch (MA_specifiedInterval)
                    {
                        case Utilities.TrendInterval.Quick:
                            {
                                Utilities.Trend stockMA_SpecifiedTrend = SelectedStock.getMATrendDirection(Utilities.TrendInterval.Quick, MA_exponential, Utilities.INTERVALQ, Index);
                                double stockTrendStrength = SelectedStock.getMATrendStrength(Utilities.TrendInterval.Quick, MA_exponential, Utilities.INTERVALQ, Index);
                                if ((stockMA_SpecifiedTrend == MA_SpecifiedTrend) && Math.Abs(stockTrendStrength) <= MA_maxStrength && Math.Abs(stockTrendStrength) >= MA_minStrength)
                                    Buy = true;
                            }
                            break;

                        case Utilities.TrendInterval.Short:
                            {
                                Utilities.Trend stockMA_SpecifiedTrend = SelectedStock.getMATrendDirection(Utilities.TrendInterval.Short, MA_exponential, Utilities.INTERVALS, Index);
                                double stockTrendStrength = SelectedStock.getMATrendStrength(Utilities.TrendInterval.Short, MA_exponential, Utilities.INTERVALS, Index);
                                if ((stockMA_SpecifiedTrend == MA_SpecifiedTrend) && Math.Abs(stockTrendStrength) <= MA_maxStrength && Math.Abs(stockTrendStrength) >= MA_minStrength)
                                    Buy = true;
                            }
                            break;

                        case Utilities.TrendInterval.Intermediate:
                            {
                                Utilities.Trend stockMA_SpecifiedTrend = SelectedStock.getMATrendDirection(Utilities.TrendInterval.Intermediate, MA_exponential, Utilities.INTERVALI, Index);
                                double stockTrendStrength = SelectedStock.getMATrendStrength(Utilities.TrendInterval.Intermediate, MA_exponential, Utilities.INTERVALI, Index);
                                if ((stockMA_SpecifiedTrend == MA_SpecifiedTrend) && Math.Abs(stockTrendStrength) <= MA_maxStrength && Math.Abs(stockTrendStrength) >= MA_minStrength)
                                    Buy = true;
                            }
                            break;

                        case Utilities.TrendInterval.Long:
                            {
                                Utilities.Trend stockMA_SpecifiedTrend = SelectedStock.getMATrendDirection(Utilities.TrendInterval.Long, MA_exponential, Utilities.INTERVALL, Index);
                                double stockTrendStrength = SelectedStock.getMATrendStrength(Utilities.TrendInterval.Long, MA_exponential, Utilities.INTERVALL, Index);
                                if ((stockMA_SpecifiedTrend == MA_SpecifiedTrend) && Math.Abs(stockTrendStrength) <= MA_maxStrength && Math.Abs(stockTrendStrength) >= MA_minStrength)
                                    Buy = true;
                            }
                            break;
                        default:
                            Console.WriteLine("Invalid trend interval selected");
                            break;
                    }

                    return Buy;


                };

                MeetsMA_XtoY_Buy = (SelectedStock, Index) =>
                {                                     
                    bool Buy = false;
                    

                    Double QMA_Value = -1, SMA_Value = -1, IMA_Value = -1, LMA_Value = -1;

                    bool QtoS = true;
                    bool StoI = true;
                    bool ItoL = true;

                    if (this.MA_exponential)
                    {
                        if (SelectedStock.MA_EMAQ.Count > 0)
                            QMA_Value = SelectedStock.MA_EMAQ[Index - Utilities.INTERVALQ + 1];

                        if (SelectedStock.MA_EMAS.Count > 0)
                            SMA_Value = SelectedStock.MA_EMAS[Index - Utilities.INTERVALS + 1];

                        if (SelectedStock.MA_EMAI.Count > 0)
                            IMA_Value = SelectedStock.MA_EMAI[Index - Utilities.INTERVALI + 1];

                        if (SelectedStock.MA_EMAL.Count > 0)
                            LMA_Value = SelectedStock.MA_EMAL[Index - Utilities.INTERVALL + 1];
                    }
                    else
                    {
                        if (SelectedStock.MA_SMAQ.Count > 0)
                            QMA_Value = SelectedStock.MA_SMAQ[Index - Utilities.INTERVALQ ];

                        if (SelectedStock.MA_SMAS.Count > 0)
                            SMA_Value = SelectedStock.MA_SMAS[Index - Utilities.INTERVALS ];

                        if (SelectedStock.MA_SMAI.Count > 0)
                            IMA_Value = SelectedStock.MA_SMAI[Index - Utilities.INTERVALI ];

                        if (SelectedStock.MA_SMAL.Count > 0)
                            LMA_Value = SelectedStock.MA_SMAL[Index - Utilities.INTERVALL ];
                    }

                    if(SMA_Value > -1)
                    switch (this.MA_QtoS)
                    {
                        case Utilities.MA_around_MA.Above:
                            if (!(QMA_Value > SMA_Value))
                                QtoS = false;
                            break;
                        case Utilities.MA_around_MA.Below:
                            if (!(QMA_Value < SMA_Value))
                                QtoS = false;
                            break;
                        case Utilities.MA_around_MA.Irrelevant:
                            //Do nothing
                            break;
                    }

                    if (IMA_Value > -1)
                    switch (this.MA_StoI)
                    {
                        case Utilities.MA_around_MA.Above:
                            if (!(SMA_Value > IMA_Value))
                                StoI = false;
                            break;
                        case Utilities.MA_around_MA.Below:
                            if (!(SMA_Value < IMA_Value))
                                StoI = false;
                            break;
                        case Utilities.MA_around_MA.Irrelevant:
                            //Do nothing
                            break;
                    }

                    if (LMA_Value > -1)
                    switch (this.MA_ItoL)
                    {
                        case Utilities.MA_around_MA.Above:
                            if (!(IMA_Value > LMA_Value))
                                ItoL = false;
                            break;
                        case Utilities.MA_around_MA.Below:
                            if (!(IMA_Value < LMA_Value))
                                ItoL = false;
                            break;
                        case Utilities.MA_around_MA.Irrelevant:
                            //Do nothing
                            break;
                    }
                    Buy = QtoS && StoI && ItoL;
                                        
                    return Buy;
                };

                MeetsStockvsMA_Buy = (SelectedStock, Index) =>
                {
                    Double MA_Value = 0;
                    Boolean Stock_to_MA = true;
                    Boolean Buy = false;

                    Double PriceClose = SelectedStock.getDailyData()[Index].getClose();

                    if (this.MA_exponential)
                    {
                        if (MA_specifiedInterval == Utilities.TrendInterval.Quick)
                            MA_Value = SelectedStock.MA_EMAQ[Index - Utilities.INTERVALQ + 1];

                        if (MA_specifiedInterval == Utilities.TrendInterval.Short)
                            MA_Value = SelectedStock.MA_EMAS[Index - Utilities.INTERVALS + 1];

                        if (MA_specifiedInterval == Utilities.TrendInterval.Intermediate)
                            MA_Value = SelectedStock.MA_EMAI[Index - Utilities.INTERVALI + 1];

                        if (MA_specifiedInterval == Utilities.TrendInterval.Long)
                            MA_Value = SelectedStock.MA_EMAL[Index - Utilities.INTERVALL + 1];
                    }
                    else
                    {
                        if (MA_specifiedInterval == Utilities.TrendInterval.Quick)
                            MA_Value = SelectedStock.MA_SMAQ[Index - Utilities.INTERVALQ + 1];

                        if (MA_specifiedInterval == Utilities.TrendInterval.Short)
                            MA_Value = SelectedStock.MA_SMAS[Index - Utilities.INTERVALS + 1];

                        if (MA_specifiedInterval == Utilities.TrendInterval.Intermediate)
                            MA_Value = SelectedStock.MA_SMAI[Index - Utilities.INTERVALI + 1];

                        if (MA_specifiedInterval == Utilities.TrendInterval.Long)
                            MA_Value = SelectedStock.MA_SMAL[Index - Utilities.INTERVALL + 1];
                    }

                    switch (MA_StocktoMA)
                    {
                        case Utilities.Stock_around_MA.Above:
                            if (!(PriceClose > MA_Value))
                                Stock_to_MA = false;
                            break;
                        case Utilities.Stock_around_MA.Below:
                            if (!(PriceClose < MA_Value))
                                Stock_to_MA = false;
                            break;
                        case Utilities.Stock_around_MA.Irrelevant:
                            //Do nothing                                
                            break;
                    }

                    Buy = Stock_to_MA;
                    
                    return Buy;
                };
                
                
                MeetsDojiRequirements_Buy = (SingleStock, StochIndex) =>
                    { 
                        Boolean DojiReqsMet = false;
                        List<EnhancedSimpleStockPoint> dailyData = SingleStock.getDailyData();

                        bool dojiIsStar = Doji.isDojiStar(dailyData, StochIndex);
                        bool dojiIsDragonFly = Doji.isDragonFly(dailyData, StochIndex);
                        bool dojiIsGraveStone = Doji.isGraveStone(dailyData, StochIndex);
                        bool dojiIsPierce = Doji.isPiercing(dailyData, StochIndex);
                        bool dojiIsBearishEngulf = Doji.isBearishEngulfing(dailyData, StochIndex);
                        bool dojiIsBullishEngulf = Doji.isBullishEngulfing(dailyData, StochIndex);
                        bool dojiIsDarkCloud = Doji.isDarkCloud(dailyData, StochIndex);

                        if (((dojiStar == true) && (dojiIsStar)) ||
                            ((dojiDragonFly == true ) && (dojiIsDragonFly)) ||
                            ((dojiGraveStone == true ) && (dojiIsGraveStone)) ||
                            ((dojiPierce == true) && (dojiIsPierce)) ||
                            ((dojiBearishEngulf == true) && (dojiIsBearishEngulf)) ||
                            ((dojiBullishEngulf == true) && (dojiIsBullishEngulf)) ||
                            ((dojiDarkCloud == true) && (dojiIsDarkCloud)))
                                DojiReqsMet = true;

                        return DojiReqsMet;
                    };

                
            }

            if (TradeType == Utilities.TradeType.Sell)
            {
                switch (BollingerBandRange)
                {
                    case Utilities.BandZones.UpperBand:
                        MeetsBollingerRequirement_Sell = (PriceHighsAndLows, BandTopAndBottom, MAValue) =>
                        { return PriceHighsAndLows[0] > BandTopAndBottom[0]; };
                        break;

                    case Utilities.BandZones.LowerBand:
                        MeetsBollingerRequirement_Sell = (PriceHighsAndLows, BandTopAndBottom, MAValue) =>
                        { return PriceHighsAndLows[1] < BandTopAndBottom[1]; };
                        break;

                    //Price_Low < MA_Value && Price_High > MA_Value
                    case Utilities.BandZones.Middle:
                        MeetsBollingerRequirement_Sell = (PriceHighsAndLows, BandTopAndBottom, MAValue) =>
                        { return PriceHighsAndLows[0] > MAValue && PriceHighsAndLows[1] < MAValue; };
                        break;
                    default:
                        throw new Exception("Bollinger Error!");
                        break;
                }

                switch (ForceType)
                {
                    case Utilities.ForceType.Bullish:
                        MeetsForceRequirements_Sell = (forceValue) =>
                        { return (forceValue > 0); };
                        break;
                    case Utilities.ForceType.Bearish:
                        MeetsForceRequirements_Sell = (forceValue) =>
                        { return (forceValue < 0); };
                        break;
                    default:
                        throw new Exception("Force Error");
                        break;

                }

                switch (HeikenAshiType)
                {
                    default:
                        MeetsHeikenAshiRequirement_Sell = (HeikenAshiStockPoint, DesiredHA) =>
                        { return HeikenAshiStockPoint.getHeikenAshiSignal() == DesiredHA; };
                        break;
                }

                switch (MACD_Around_Signal)
                {
                    case Utilities.MACD_around_Signal.Above:
                        MeetsMACD_AroundSignal_Requirements_Sell = (MacdValue, MacdSignalValue) =>
                        { return MacdValue > MacdSignalValue; };
                        break;
                    case Utilities.MACD_around_Signal.Below:
                        MeetsMACD_AroundSignal_Requirements_Sell = (MacdValue, MacdSignalValue) =>
                        { return MacdValue < MacdSignalValue; };
                        break;
                    case Utilities.MACD_around_Signal.Irrelevant:
                        MeetsMACD_AroundSignal_Requirements_Sell = (MacdValue, MacdSignalValue) =>
                        { return true; };
                        break;
                    default:
                        throw new Exception("MACD Around Signal Error");
                        break;
                }

                switch (MACD_Around_Zero)
                {
                    case Utilities.MACD_around_Zero.Above:
                        MeetsMACD_AroundZero_Requirements_Sell = (MacdValue, MacdSignalValue) =>
                        { return MacdValue > 0; };
                        break;
                    case Utilities.MACD_around_Zero.Below:
                        MeetsMACD_AroundZero_Requirements_Sell = (MacdValue, MacdSignalValue) =>
                        { return MacdValue < 0; };
                        break;
                    case Utilities.MACD_around_Zero.Irrelevant:
                        MeetsMACD_AroundZero_Requirements_Sell = (MacdValue, MacdSignalValue) =>
                        { return true; };
                        break;
                    default:
                        throw new Exception("MACD Around Zero Error");
                        break;
                }

                switch (StochasticType)
                {
                    case Utilities.StochasticType.Bullish:
                        MeetsStochasticRequirements_Sell = (SingleStock, StochIndex, StochSMAIndex) =>
                        { return SingleStock.Stochastic_Oversold(StochIndex, StochSMAIndex); };
                        break;
                    case Utilities.StochasticType.Bearish:
                        MeetsStochasticRequirements_Sell = (SingleStock, StochIndex, StochSMAIndex) =>
                        { return SingleStock.Stochastic_Overbought(StochIndex, StochSMAIndex); };
                        break;
                }

                switch (RSIType)
                {
                    case Utilities.RSIType.Bullish:
                        MeetsRSIRequirements_Sell = (SingleStock, RSIIndex) =>
                        { return SingleStock.RSI_OverSold(RSIIndex); };
                        break;
                    case Utilities.RSIType.Bearish:
                        MeetsRSIRequirements_Sell = (SingleStock, RSIIndex) =>
                        { return SingleStock.RSI_OverBought(RSIIndex); };
                        break;
                }

                switch (ADXTrend)
                {
                    case Utilities.ADXTrend.NoTrend:
                        MeetsADXRequirements_Sell = (currentADX) =>
                        { return !(currentADX > Utilities.ADX_normal_trend_line); };
                        break;

                    case Utilities.ADXTrend.Normal:
                        MeetsADXRequirements_Sell = (currentADX) =>
                        { return (currentADX > Utilities.ADX_normal_trend_line); };
                        break;

                    case Utilities.ADXTrend.Strong:
                        MeetsADXRequirements_Sell = (currentADX) =>
                        { return (currentADX > Utilities.ADX_strong_trend_line); };
                        break;

                    case Utilities.ADXTrend.Extreme:
                        MeetsADXRequirements_Sell = (currentADX) =>
                        { return (currentADX > Utilities.ADX_extreme_trend_line); };
                        break;
                    case Utilities.ADXTrend.Irrelevant:
                        MeetsADXRequirements_Sell = (currentADX) =>
                        { return true; };
                        break;
                }

                switch (ADX_PDIvsNDI)
                {
                    case Utilities.ADX_PDIvsNDI.PDI_above_NDI:
                        MeetsDIRequirements_Sell = (currentPDI, currentNDI) =>
                        { return (currentPDI > currentNDI); };
                        break;
                    case Utilities.ADX_PDIvsNDI.NDI_above_PDI:
                        MeetsDIRequirements_Sell = (currentPDI, currentNDI) =>
                        { return (currentNDI > currentPDI); };
                        break;
                    case Utilities.ADX_PDIvsNDI.Irrelevant:
                        MeetsDIRequirements_Sell = (currentPDI, currentNDI) =>
                        { return true; };
                        break;
                }

                switch (ADXTrending)
                {
                    case Utilities.ADX_Trending.Trending:
                        MeetsADXTrendingRequirements_Sell = (ADX, Index) =>
                        {
                            int TrendStrength = 2;
                            for (int j = Index; j > Index - TrendStrength; j--)
                            {
                                if (ADX.Points[j].YValues[0] < ADX.Points[j - 1].YValues[0])
                                    return false;
                            }
                            return true;
                        };
                        break;
                    case Utilities.ADX_Trending.NotTrending:
                        MeetsADXTrendingRequirements_Sell = (ADX, Index) =>
                        { return ADX.Points[Index].YValues[0] <= ADX.Points[Index - 1].YValues[0]; };
                        break;
                    case Utilities.ADX_Trending.Irrelevent:
                        MeetsADXTrendingRequirements_Sell = (SingleStock, Index) =>
                        { return true; };
                    break;
                }

                MeetsMA_XtoY_Sell = (SelectedStock, Index) =>
                {
                    bool Sell = false;

                    Double QMA_Value = -1, SMA_Value = -1, IMA_Value = -1, LMA_Value = -1;

                    bool QtoS = true;
                    bool StoI = true;
                    bool ItoL = true;

                    if (this.MA_exponential)
                    {
                        if (SelectedStock.MA_EMAQ.Count > 0)
                            QMA_Value = SelectedStock.MA_EMAQ[Index - Utilities.INTERVALQ + 1];

                        if (SelectedStock.MA_EMAS.Count > 0)
                            SMA_Value = SelectedStock.MA_EMAS[Index - Utilities.INTERVALS + 1];

                        if (SelectedStock.MA_EMAI.Count > 0)
                            IMA_Value = SelectedStock.MA_EMAI[Index - Utilities.INTERVALI + 1];

                        if (SelectedStock.MA_EMAL.Count > 0)
                            LMA_Value = SelectedStock.MA_EMAL[Index - Utilities.INTERVALL + 1];
                    }
                    else
                    {
                        if (SelectedStock.MA_SMAQ.Count > 0)
                            QMA_Value = SelectedStock.MA_SMAQ[Index - Utilities.INTERVALQ];

                        if (SelectedStock.MA_SMAS.Count > 0)
                            SMA_Value = SelectedStock.MA_SMAS[Index - Utilities.INTERVALS];

                        if (SelectedStock.MA_SMAI.Count > 0)
                            IMA_Value = SelectedStock.MA_SMAI[Index - Utilities.INTERVALI];

                        if (SelectedStock.MA_SMAL.Count > 0)
                            LMA_Value = SelectedStock.MA_SMAL[Index - Utilities.INTERVALL];
                    }

                    if (SMA_Value > -1)
                        switch (this.MA_QtoS)
                        {
                            case Utilities.MA_around_MA.Above:
                                if (!(QMA_Value > SMA_Value))
                                    QtoS = false;
                                break;
                            case Utilities.MA_around_MA.Below:
                                if (!(QMA_Value < SMA_Value))
                                    QtoS = false;
                                break;
                            case Utilities.MA_around_MA.Irrelevant:
                                //Do nothing
                                break;
                        }

                    if (IMA_Value > -1)
                        switch (this.MA_StoI)
                        {
                            case Utilities.MA_around_MA.Above:
                                if (!(SMA_Value > IMA_Value))
                                    StoI = false;
                                break;
                            case Utilities.MA_around_MA.Below:
                                if (!(SMA_Value < IMA_Value))
                                    StoI = false;
                                break;
                            case Utilities.MA_around_MA.Irrelevant:
                                //Do nothing
                                break;
                        }

                    if (LMA_Value > -1)
                        switch (this.MA_ItoL)
                        {
                            case Utilities.MA_around_MA.Above:
                                if (!(IMA_Value > LMA_Value))
                                    ItoL = false;
                                break;
                            case Utilities.MA_around_MA.Below:
                                if (!(IMA_Value < LMA_Value))
                                    ItoL = false;
                                break;
                            case Utilities.MA_around_MA.Irrelevant:
                                //Do nothing
                                break;
                        }
                    Sell = QtoS && StoI && ItoL;

                    return Sell;
                };

                MeetsStockvsMA_Sell = (SelectedStock, Index) =>
                {
                    Double MA_Value = 0;
                    Boolean Stock_to_MA = true;
                    Boolean Sell = false;

                    Double PriceClose = SelectedStock.getDailyData()[Index].getClose();

                    if (this.MA_exponential)
                    {
                        if (MA_specifiedInterval == Utilities.TrendInterval.Quick)
                            MA_Value = SelectedStock.MA_EMAQ[Index - Utilities.INTERVALQ + 1];

                        if (MA_specifiedInterval == Utilities.TrendInterval.Short)
                            MA_Value = SelectedStock.MA_EMAS[Index - Utilities.INTERVALS + 1];

                        if (MA_specifiedInterval == Utilities.TrendInterval.Intermediate)
                            MA_Value = SelectedStock.MA_EMAI[Index - Utilities.INTERVALI + 1];

                        if (MA_specifiedInterval == Utilities.TrendInterval.Long)
                            MA_Value = SelectedStock.MA_EMAL[Index - Utilities.INTERVALL + 1];
                    }
                    else
                    {
                        if (MA_specifiedInterval == Utilities.TrendInterval.Quick)
                            MA_Value = SelectedStock.MA_SMAQ[Index - Utilities.INTERVALQ + 1];

                        if (MA_specifiedInterval == Utilities.TrendInterval.Short)
                            MA_Value = SelectedStock.MA_SMAS[Index - Utilities.INTERVALS + 1];

                        if (MA_specifiedInterval == Utilities.TrendInterval.Intermediate)
                            MA_Value = SelectedStock.MA_SMAI[Index - Utilities.INTERVALI + 1];

                        if (MA_specifiedInterval == Utilities.TrendInterval.Long)
                            MA_Value = SelectedStock.MA_SMAL[Index - Utilities.INTERVALL + 1];
                    }

                    switch (MA_StocktoMA)
                    {
                        case Utilities.Stock_around_MA.Above:
                            if (!(PriceClose > MA_Value))
                                Stock_to_MA = false;
                            break;
                        case Utilities.Stock_around_MA.Below:
                            if (!(PriceClose < MA_Value))
                                Stock_to_MA = false;
                            break;
                        case Utilities.Stock_around_MA.Irrelevant:
                            //Do nothing                                
                            break;
                    }

                    Sell = Stock_to_MA;

                    return Sell;
                };

                MeetsMATrendStrengthRequirements_Sell = (SelectedStock, Index) =>
                {
                    Boolean Buy = false;
                    if (MA_maxStrength >= MA_minStrength)
                        return true;

                    if (MA_SpecifiedTrend == Utilities.Trend.Irrelevant)
                        return true;

                    switch (MA_specifiedInterval)
                    {
                        case Utilities.TrendInterval.Quick:
                            {
                                Utilities.Trend stockMA_SpecifiedTrend = SelectedStock.getMATrendDirection(Utilities.TrendInterval.Quick, MA_exponential, Utilities.INTERVALQ, Index);
                                double stockTrendStrength = SelectedStock.getMATrendStrength(Utilities.TrendInterval.Quick, MA_exponential, Utilities.INTERVALQ, Index);
                                if ((stockMA_SpecifiedTrend == MA_SpecifiedTrend) && Math.Abs(stockTrendStrength) <= MA_maxStrength && Math.Abs(stockTrendStrength) >= MA_minStrength)
                                    Buy = true;
                            }
                            break;

                        case Utilities.TrendInterval.Short:
                            {
                                Utilities.Trend stockMA_SpecifiedTrend = SelectedStock.getMATrendDirection(Utilities.TrendInterval.Short, MA_exponential, Utilities.INTERVALS, Index);
                                double stockTrendStrength = SelectedStock.getMATrendStrength(Utilities.TrendInterval.Short, MA_exponential, Utilities.INTERVALS, Index);
                                if ((stockMA_SpecifiedTrend == MA_SpecifiedTrend) && Math.Abs(stockTrendStrength) <= MA_maxStrength && Math.Abs(stockTrendStrength) >= MA_minStrength)
                                    Buy = true;
                            }
                            break;

                        case Utilities.TrendInterval.Intermediate:
                            {
                                Utilities.Trend stockMA_SpecifiedTrend = SelectedStock.getMATrendDirection(Utilities.TrendInterval.Intermediate, MA_exponential, Utilities.INTERVALI, Index);
                                double stockTrendStrength = SelectedStock.getMATrendStrength(Utilities.TrendInterval.Intermediate, MA_exponential, Utilities.INTERVALI, Index);
                                if ((stockMA_SpecifiedTrend == MA_SpecifiedTrend) && Math.Abs(stockTrendStrength) <= MA_maxStrength && Math.Abs(stockTrendStrength) >= MA_minStrength)
                                    Buy = true;
                            }
                            break;

                        case Utilities.TrendInterval.Long:
                            {
                                Utilities.Trend stockMA_SpecifiedTrend = SelectedStock.getMATrendDirection(Utilities.TrendInterval.Long, MA_exponential, Utilities.INTERVALL, Index);
                                double stockTrendStrength = SelectedStock.getMATrendStrength(Utilities.TrendInterval.Long, MA_exponential, Utilities.INTERVALL, Index);
                                if ((stockMA_SpecifiedTrend == MA_SpecifiedTrend) && Math.Abs(stockTrendStrength) <= MA_maxStrength && Math.Abs(stockTrendStrength) >= MA_minStrength)
                                    Buy = true;
                            }
                            break;
                        default:
                            Console.WriteLine("Invalid trend interval selected");
                            break;
                    }

                    return Buy;


                };

                MeetsDojiRequirements_Sell = (SingleStock, StochIndex) =>
                {
                    Boolean DojiReqsMet = false;
                    List<EnhancedSimpleStockPoint> dailyData = SingleStock.getDailyData();

                    bool dojiIsStar = Doji.isDojiStar(dailyData, StochIndex);
                    bool dojiIsDragonFly = Doji.isDragonFly(dailyData, StochIndex);
                    bool dojiIsGraveStone = Doji.isGraveStone(dailyData, StochIndex);
                    bool dojiIsPierce = Doji.isPiercing(dailyData, StochIndex);
                    bool dojiIsBearishEngulf = Doji.isBearishEngulfing(dailyData, StochIndex);
                    bool dojiIsBullishEngulf = Doji.isBullishEngulfing(dailyData, StochIndex);
                    bool dojiIsDarkCloud = Doji.isDarkCloud(dailyData, StochIndex);

                    if (((dojiStar == true) && (dojiIsStar)) ||
                        ((dojiDragonFly == true) && (dojiIsDragonFly)) ||
                        ((dojiGraveStone == true) && (dojiIsGraveStone)) ||
                        ((dojiPierce == true) && (dojiIsPierce)) ||
                        ((dojiBearishEngulf == true) && (dojiIsBearishEngulf)) ||
                        ((dojiBullishEngulf == true) && (dojiIsBullishEngulf)) ||
                        ((dojiDarkCloud == true) && (dojiIsDarkCloud)))
                            DojiReqsMet = true;

                    return DojiReqsMet;
                };
            }

        }
 
    }
}
