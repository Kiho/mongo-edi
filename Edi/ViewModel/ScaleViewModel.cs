namespace Edi.ViewModel
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using Edi.ViewModel.Base;
  using System.Globalization;

  internal class ScaleViewModel : ViewModelBase
  {
    #region fields
    /// <summary>
    /// This equal 100% of actual size is scaled relative to this value
    /// </summary>
    private int mHundredPercentSize;

    /// <summary>
    /// This is used to initialize scaling without rounding errors
    /// </summary>
    private int mViewScale = 100;

    // Select a scale of a font to display a document's content
    private int mSelectedFontSize;

    // Collection to choose scale of font used to display a document
    private Dictionary<string, int> mScale = null;
    #endregion fields

    #region constructor
    public ScaleViewModel(int hundredPercentSize)
    {
      this.mHundredPercentSize = mSelectedFontSize = hundredPercentSize;
    }

    protected ScaleViewModel()
    {
    }
    #endregion constructor

    #region property
    public Dictionary<string, int> ScaleList
    {
      get
      {
        if (this.mScale == null)
        {
          this.mScale = new Dictionary<string, int>();

          double[] scaleSize = { 0.25, 0.5, 1, 1.25, 1.50, 1.75, 2.00, 4.00 };

          int lastFontSz = -1;

          for (int i = scaleSize.Length-1; i > 0; i--)
          {
            int scale = (int)(scaleSize[i] * 100);
            int fontSz = (int)Math.Round(mHundredPercentSize * scaleSize[i], 0);

            this.mScale.Add(string.Format(CultureInfo.CurrentCulture, "{0} %", scale), fontSz);

            if (lastFontSz > 0)
            {
              if (this.mSelectedFontSize < lastFontSz && this.mSelectedFontSize > fontSz)
              {
                scale = this.mViewScale;
                fontSz = this.mSelectedFontSize;

                this.mScale.Add(string.Format(CultureInfo.CurrentCulture, "{0} %", scale), fontSz);
              }
            }

            lastFontSz = fontSz;
          }
        }

        return this.mScale;
      }
    }

    public int SelectedFontSize
    {
      get
      {
        return this.mSelectedFontSize;
      }

      set
      {
        if (this.mSelectedFontSize != value)
        {
          this.mSelectedFontSize = value;
          this.NotifyPropertyChanged(() => this.SelectedFontSize);
        }
      }
    }

    public void SelectedFontSizeCompute(int iViewScale)
    {
      if (iViewScale <= 25)
      {
        this.SelectedFontSize = mHundredPercentSize / 25;            // Return minimum font size of 3 for 25 %
        return;
      }

      if (iViewScale >= 1000)
      {
        this.SelectedFontSize = mHundredPercentSize * 10;          // Return maximum font size of 120 for 1000 %
        return;
      }

      this.mViewScale = iViewScale;
      this.SelectedFontSize = (int)Math.Round(((iViewScale / 100.0) * mHundredPercentSize), 0);
    }
    #endregion property
  }
}
