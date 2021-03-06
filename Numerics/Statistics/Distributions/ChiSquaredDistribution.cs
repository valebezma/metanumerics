﻿using System;
using System.Collections.Generic;

using Meta.Numerics.Functions;

namespace Meta.Numerics.Statistics.Distributions {


    /// <summary>
    /// Represents a &#x3C7;<sup>2</sup> distribution.
    /// </summary>
    /// <remarks>
    /// <para>A chi squared distribution is an asymmetrical distribution ranging from zero to infinity with a peak near its
    /// number of degrees of freedom &#x3BD;. It is a one-parameter distribution determined entirely by the parameter nu.</para>
    /// <img src="../images/ChiSquaredPlot.png" />
    /// <para>The figure above shows the &#x3C7;<sup>2</sup> distribution for &#x3BD; = 6, as well as the normal distribution
    /// with equal mean and variance for reference.</para>
    /// <para>The sum of the squares of &#x3BD; independent standard-normal distributed variables is distributed as &#x3C7;<sup>2</sup>
    /// with &#x3BD; degrees of freedom.</para>
    /// <img src="../images/ChiSquaredFromNormal.png" />
    /// <para>The &#x3C7;<sup>2</sup> distribution appears in least-squares fitting as the distribution of the sum-of-squared-deviations
    /// under the null hypothesis that the model explains the data. For example, the goodness-of-fit statistic returned by the
    /// model our model fitting methods (<see cref="UncertainMeasurementSample{T}.FitToFunction"/>, <see cref="UncertainMeasurementSample{T}.FitToLinearFunction"/>,
    /// <see cref="UncertainMeasurementSample.FitToLine"/>, and others) follows a &#x3C7;<sup>2</sup> distribution.</para>
    /// </remarks>
    /// <seealso href="http://en.wikipedia.org/wiki/Chi-square_distribution" />
    public sealed class ChiSquaredDistribution : ContinuousDistribution {

        // internally, we use our Gamma distribution machinery to do our heavy lifting

        private readonly double nu;
        private readonly GammaDistribution gamma;

        /// <summary>
        /// Initializes a new &#x3C7;<sup>2</sup> distribution.
        /// </summary>
        /// <param name="nu">The number of degrees of freedom, which must be positive.</param>
        public ChiSquaredDistribution (double nu) {
            if (nu < 1.0) throw new ArgumentOutOfRangeException(nameof(nu));
            this.nu = nu;
            this.gamma = new GammaDistribution(nu / 2.0);
        }

        /// <summary>
        /// Gets the number of degrees of freedom &#x3BD; of the distribution.
        /// </summary>
        public double DegreesOfFreedom {
            get {
                return (nu);
            }
        }

        /// <inheritdoc />
        public override double ProbabilityDensity (double x) {
            return (gamma.ProbabilityDensity(x / 2.0) / 2.0);
        }

        /// <inheritdoc />
        public override double LeftProbability (double x) {
            return (gamma.LeftProbability(x / 2.0));
        }

        /// <inheritdoc />
        public override double RightProbability (double x) {
            return (gamma.RightProbability(x / 2.0));
        }

        /// <inheritdoc />
        public override double InverseLeftProbability (double P) {
            return (2.0 * gamma.InverseLeftProbability(P));
        }

        // improve this
        /// <inheritdoc />
        public override double RawMoment (int r) {
            if (r < 0) {
                throw new ArgumentOutOfRangeException(nameof(r));
            } else if (r < 16) {
                // nu ( nu + 2) (nu + 4) \cdots, r times
                double m = 1.0;
                for (int k = 0; k < r; k++) {
                    m *= (nu + 2.0 * k);
                }
                return (m);
            } else {
                return (AdvancedMath.Pochhammer(nu / 2.0, r) * MoreMath.Pow(2.0, r));
            }
        }

        /// <inheritdoc />
        public override double CentralMoment (int r) {
            if (r < 0) {
                throw new ArgumentOutOfRangeException(nameof(r));
            } else if (r == 0) {
                return (1.0);
            } else if (r == 1) {
                return (0.0);
            } else {
                // use C_{r} = 2^r U(-r, 1-r-\nu/2, -\nu/2) where U is irregular confluent hypergeometric
                // use recursion U(a-1,b-1,z) = (1-b+z) U(a,b,z) + z a U(a+1,b+1,z) to derive
                // C_{n+1} = 2n (C_{n} + \nu C_{n-1})
                double C1 = 0.0;
                double C2 = 2.0 * nu;
                for (int k = 2; k < r; k++) {
                    double C3 = (2*k) * (C2 + nu * C1);
                    C1 = C2;
                    C2 = C3;
                }
                return(C2);
            }
        }

        /// <inheritdoc />
        public override double Cumulant (int r) {
            if (r < 0) {
                throw new ArgumentOutOfRangeException(nameof(r));
            } else if (r == 0) {
                return (0.0);
            } else {
                return (MoreMath.Pow(2, r - 1) * AdvancedIntegerMath.Factorial(r - 1) * nu);
            }
        }

        /// <inheritdoc />
        public override double Mean {
            get {
                return (nu);
            }
        }

        /// <inheritdoc />
        public override double Variance {
            get {
                return (2.0 * nu);
            }
        }

        /// <inheritdoc />
        public override double Median {
            get {
                // start with an approximation
                double xm = nu - (2.0 / 3.0) + (4.0 / 27.0) / nu - (8.0 / 729.0) / (nu * nu);
                // polish it using Newton's method
                while (true) {
                    double dx = (0.5 - LeftProbability(xm)) / ProbabilityDensity(xm);
                    xm += dx;
                    if (Math.Abs(dx) <= Global.Accuracy * xm) break;
                }
                return (xm);
            }
        }

        /// <inheritdoc />
        public override double Skewness {
            get {
                return (Math.Sqrt(8.0 / nu));
            }
        }

        /// <inheritdoc />
        public override double ExcessKurtosis {
            get {
                return (12.0 / nu);
            }
        }

        /// <inheritdoc />
        public override Interval Support {
            get {
                return (Interval.FromEndpoints(0.0, Double.PositiveInfinity));
            }
        }

    }

}