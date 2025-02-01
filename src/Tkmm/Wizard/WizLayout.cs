using Tkmm.Wizard.Actions;
using Tkmm.Wizard.ViewModels;
using Tkmm.Wizard.Views;
using WizDumpConfigPage = Tkmm.Wizard.Views.WizDumpConfigPage;
using WizEmulatorSelectionPage = Tkmm.Wizard.Views.WizEmulatorSelectionPage;
    
namespace Tkmm.Wizard;

public class WizLayout
{
    public static WizPageViewModel FirstPage {
        get {
            WizPageViewModel result = new(-1, null, null, string.Empty, []);
            UpdatePage(result, _page0);
            return result;
        }
    }
        
    private static readonly WizPageViewModel _page0 = new(
        id: 0, lastPage: null,
        TkLocale.WizPage0_Title, TkLocale.WizPage0_Description, [
            new WizAction(TkLocale.WizPage0_Action, 0)
        ]);

    private static readonly WizPageViewModel _page1 = new(
        id: 1, lastPage: FirstPage,
        TkLocale.WizPage1_Title, WizEmulatorSelectionPage.Instance, [
            new WizAction(TkLocale.WizPage1_Action_Next, 1, WizEmulatorSelectionPage.Instance.CheckSelection)
        ]);
    
    private static readonly WizPageViewModel _pageRyujinxSetup = new(
        id: 3, lastPage: _page1,
        TkLocale.WizPageRyujinxSetup_Title, TkLocale.WizPageRyujinxSetup_Description, [
            new WizAction(TkLocale.WizPageRyujinxSetup_Action_Start, 0, WizActions.StartRyujinxSetup)
        ]);
    
    private static readonly WizPageViewModel _page2 = new(
        id: 2, lastPage: _page1,
        TkLocale.WizPage2_Title, new WizDumpConfigPage(), [
            new WizAction(TkLocale.WizPage2_Action_Verify, 0, WizActions.VerifyConfig)
        ]);
    
    private static readonly WizPageViewModel _pageFinal = new(
        id: -1, lastPage: _page1,
        TkLocale.WizPageFinal_Title, new WizLangConfigPage(), [
            new WizAction(TkLocale.WizPageFinal_Action_Finish, 0, WizActions.CompleteSetup)
        ]);

    public static void NextPage(WizPageViewModel current, int selection)
    {
        WizPageViewModel nextPage = GetNextPage(current, selection);
        UpdatePage(current, nextPage);
    }

    public static void UpdatePage(WizPageViewModel current, WizPageViewModel target)
    {
        current.Id = target.Id;
        current.LastPage = target.LastPage;
        current.Title = target.Title;
        current.PageContent = target.PageContent;
        current.Actions = target.Actions;
    }
    
    private static WizPageViewModel GetNextPage(WizPageViewModel current, int selection)
    {
        return (current.Id, selection) switch {
            (0, _) => _page1,
            (1, 0) => _pageRyujinxSetup,
            (1, 1) => _page2,
            (1, 2) => _pageFinal,
            _ => _pageFinal
        };
    }
}