namespace Edi.AvalonEdit.Intellisense
{
  using System.Collections.Generic;
  using ICSharpCode.AvalonEdit.CodeCompletion;

  public interface ICompletionDataProvider
	{
		IEnumerable<ICompletionData> GetData(string text, int position, string input, string highlightingName); 
	}
}