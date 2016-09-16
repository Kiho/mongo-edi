namespace Edi.AvalonEdit.Intellisense
{
  using ICSharpCode.AvalonEdit.CodeCompletion;

  public interface ICompletionWindowResolver
	{
		CompletionWindow Resolve();
	}
}