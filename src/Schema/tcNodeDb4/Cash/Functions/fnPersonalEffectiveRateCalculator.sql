CREATE FUNCTION Cash.fnPersonalEffectiveRateCalculator
(
    @Profit decimal(18, 2)
)
RETURNS decimal(18, 5)
AS
/*
Let:
  PA = 12570                     // Personal Allowance
  BR = 50270                     // Upper limit of basic rate band
  HR = 100000                    // Income where PA starts to taper
  AR = 125140                    // Income where PA is fully lost

  IT_basic = 0.20                // Income Tax basic rate
  IT_higher = 0.40               // Income Tax higher rate

  NI_main = 0.09                 // Class 4 NI main rate
  NI_upper = 0.02                // Class 4 NI upper rate
  NI_threshold = 12570           // Class 4 NI lower profits limit
  NI_upper_limit = 50270         // Class 4 NI upper profits limit

Formula:
  TaxableIncome = MAX(0, Profit - PA_adjusted)

  Where:
    If Profit <= HR:
        PA_adjusted = PA
    Else:
        PA_adjusted = MAX(0, PA - (Profit - HR) / 2)

  IncomeTax =
      IT_basic * MIN(TaxableIncome, BR - PA_adjusted)
    + IT_higher * MAX(0, TaxableIncome - (BR - PA_adjusted))

  Class4NI =
      NI_main * MIN(MAX(0, Profit - NI_threshold), NI_upper_limit - NI_threshold)
    + NI_upper * MAX(0, Profit - NI_upper_limit)

  EffectiveRate = (IncomeTax + Class4NI) / Profit
*/
BEGIN
    --------------------------------------------------------------------
    -- Constants (mirroring UK Income Tax + Class 4 NI structure)
    --------------------------------------------------------------------
    DECLARE 
        @PA              decimal(18, 2) = 12570,     -- Personal Allowance
        @BR              decimal(18, 2) = 50270,     -- Basic rate upper limit
        @HR              decimal(18, 2) = 100000,    -- PA taper start
        @AR              decimal(18, 2) = 125140,    -- PA fully removed

        @IT_basic        decimal(18, 5) = 0.20,      -- Income Tax basic rate
        @IT_higher       decimal(18, 5) = 0.40,      -- Income Tax higher rate

        @NI_main         decimal(18, 5) = 0.09,      -- Class 4 NI main rate
        @NI_upper        decimal(18, 5) = 0.02,      -- Class 4 NI upper rate
        @NI_threshold    decimal(18, 2) = 12570,     -- NI lower profits limit
        @NI_upper_limit  decimal(18, 2) = 50270;     -- NI upper profits limit

    --------------------------------------------------------------------
    -- Adjust Personal Allowance for tapering
    --------------------------------------------------------------------
    DECLARE @PA_adjusted decimal(18, 2);

    IF @Profit <= @HR
        SET @PA_adjusted = @PA;
    ELSE
        SET @PA_adjusted = CASE 
                               WHEN @PA - (@Profit - @HR) / 2 < 0 
                                   THEN 0
                                   ELSE @PA - (@Profit - @HR) / 2
                           END;

    --------------------------------------------------------------------
    -- Taxable Income
    --------------------------------------------------------------------
    DECLARE @TaxableIncome decimal(18, 2) =
        CASE WHEN @Profit - @PA_adjusted > 0 
             THEN @Profit - @PA_adjusted 
             ELSE 0 
        END;

    --------------------------------------------------------------------
    -- Income Tax calculation
    --------------------------------------------------------------------
    DECLARE @IncomeTax decimal(18, 5) =
        @IT_basic * 
            CASE WHEN @TaxableIncome < (@BR - @PA_adjusted)
                 THEN @TaxableIncome
                 ELSE (@BR - @PA_adjusted)
            END
        +
        @IT_higher *
            CASE WHEN @TaxableIncome > (@BR - @PA_adjusted)
                 THEN @TaxableIncome - (@BR - @PA_adjusted)
                 ELSE 0
            END;

    --------------------------------------------------------------------
    -- Class 4 National Insurance
    --------------------------------------------------------------------
    DECLARE @Class4NI decimal(18, 5) =
        @NI_main *
            CASE WHEN @Profit > @NI_threshold
                 THEN 
                     CASE WHEN @Profit < @NI_upper_limit
                          THEN @Profit - @NI_threshold
                          ELSE @NI_upper_limit - @NI_threshold
                     END
                 ELSE 0
            END
        +
        @NI_upper *
            CASE WHEN @Profit > @NI_upper_limit
                 THEN @Profit - @NI_upper_limit
                 ELSE 0
            END;

    --------------------------------------------------------------------
    -- Effective Rate
    --------------------------------------------------------------------
    DECLARE @EffectiveRate decimal(18, 5);

    IF @Profit <= 0
        SET @EffectiveRate = 0;
    ELSE
        SET @EffectiveRate = (@IncomeTax + @Class4NI) / @Profit;

    RETURN @EffectiveRate;
END;
