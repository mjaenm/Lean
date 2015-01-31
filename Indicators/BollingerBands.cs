﻿using System;

namespace QuantConnect.Indicators
{
    public class BollingerBands : Indicator
    {
        /// <summary>
        /// Gets the type of moving average
        /// </summary>
        public MovingAverageType MovingAverageType { get; private set; }

        /// <summary>
        /// Gets the standard deviation
        /// </summary>
        public IndicatorBase<IndicatorDataPoint> StandardDeviation { get; private set; }

        /// <summary>
        /// Gets the middle bollinger band (moving average)
        /// </summary>
        public IndicatorBase<IndicatorDataPoint> MiddleBand { get; private set; }

        /// <summary>
        /// Gets the upper bollinger band (middleBand + k * stdDev)
        /// </summary>
        public IndicatorBase<IndicatorDataPoint> UpperBand { get; private set; }

        /// <summary>
        /// Gets the upper bollinger band (middleBand - k * stdDev)
        /// </summary>
        public IndicatorBase<IndicatorDataPoint> LowerBand { get; private set; }

        /// <summary>
        /// Initializes a new instance of the BollingerBands class
        /// </summary>
        /// <param name="maPeriod">The period of the moving average</param>
        /// <param name="stdPeriod">The period of the standard deviation</param>
        /// <param name="k">The number of standard deviations specifying the distance of bands from the moving average</param>
        /// <param name="movingAverageType">The type of moving average to be used</param>
        public BollingerBands(int maPeriod, int stdPeriod, int k, MovingAverageType movingAverageType = MovingAverageType.Simple)
            : this(string.Format("BOL({0},{1},{2})", maPeriod, stdPeriod, k), maPeriod, stdPeriod, k, movingAverageType)
        {
        }

        /// <summary>
        /// Initializes a new instance of the BollingerBands class
        /// </summary>
        /// <param name="period">The period of the standard deviation and moving average</param>
        /// <param name="k">The number of standard deviations specifying the distance of bands from the moving average</param>
        /// <param name="movingAverageType">The type of moving average to be used</param>
        public BollingerBands(int period, int k, MovingAverageType movingAverageType = MovingAverageType.Simple)
            : this(string.Format("BOL({0},{1})", period, k), period, k, movingAverageType)
        {
        }

        /// <summary>
        /// Initializes a new instance of the BollingerBands class
        /// </summary>
        /// <param name="name">The name of this indicator</param>
        /// <param name="maPeriod">The period of the moving average</param>
        /// <param name="stdPeriod">The period of the standard deviation</param>
        /// <param name="k">The number of standard deviations specifying the distance of the bands from the moving average</param>
        /// <param name="movingAverageType">The type of moving average to be used</param>
        public BollingerBands(String name, int maPeriod, int stdPeriod, int k, MovingAverageType movingAverageType = MovingAverageType.Simple)
            : base(name)
        {
            MovingAverageType = movingAverageType;
            StandardDeviation = new StandardDeviation(name + "_StandardDeviation", stdPeriod);
            MiddleBand = movingAverageType.AsIndicator(name + "_MiddleBand", maPeriod);
            var kConstant = new ConstantIndicator<IndicatorDataPoint>(k.ToString, k);
            LowerBand = MiddleBand.Minus(StandardDeviation.Times(kConstant), name + "_LowerBand");
            UpperBand = MiddleBand.Plus(StandardDeviation.Times(kConstant), name + "_UpperBand");
        }

        /// <summary>
        /// Initializes a new instance of the BollingerBands class
        /// </summary>
        /// <param name="name">The name of this indicator</param>
        /// <param name="period">The period of the standard deviation and moving average</param>
        /// <param name="k">The number of standard deviations specifying the distance of the bands from the moving average</param>
        /// <param name="movingAverageType">The type of moving average to be used</param>
        public BollingerBands(String name, int period, int k, MovingAverageType movingAverageType = MovingAverageType.Simple)
            : base(name)
        {
            BollingerBands(name, period, period, k, movingAverageType);
        }

        /// <summary>
        /// Gets a flag indicating when this indicator is ready and fully initialized
        /// </summary>
        public override bool IsReady
        {
            get { return MiddleBand.IsReady && UpperBand.IsReady && LowerBand.IsReady; }
        }

        /// <summary>
        /// Z-Score = (currentValue - movingAverage)/standardDeviation
        /// </summary>
        /// <param name="input">The input given to the indicator</param>
        /// <returns>Z-Score</returns>
        private decimal ComputeZScore(IndicatorDataPoint input)
        {
            return (input.Value - MiddleBand.Current.Value) / StandardDeviation.Current.Value;
        }

        /// <summary>
        /// Computes the next value of this indicator from the given state
        /// </summary>
        /// <param name="input">The input given to the indicator</param>
        /// <returns>Z-score. The number of standard deviations between the input and the moving average</returns>
        protected override decimal ComputeNextValue(IndicatorDataPoint input)
        {
            StandardDeviation.Update(input);
            MiddleBand.Update(input);
            UpperBand.Update(input);
            LowerBand.Update(input);
            return ComputeZScore(input);
        }
    }
}
