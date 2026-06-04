# Copilot Instructions

## Project Guidelines
- Currency in BombaProMax is DH (Moroccan Dirham), never DA. Always use DH in string formats, labels, and display values. Use StringFormat '{}{0:N2} DH'.

## Development Workflow
- For BombaProMax project, follow this development workflow per prompt:
  - 1st prompt: All API side at once (Model, DTO, AutoMapper Profile, DbContext, Migration, Controller)
  - 2nd prompt: WPF side steps (ApiConfig, DTO, CardItem, Service, SectionViewModel)
  - 3rd+ prompts: One dialog/popup per prompt (NouveauXxx, then EditXxx, then DetailXxx)
- NO MAUI development — only API and WPF (FourniPro).

## WPF Dialog Style Pattern
- All popups follow this style:
  - **Window**: ui:FluentWindow, 900×720 (detail/edit) or 900×780 (create), WindowStartupLocation=CenterOwner, ResizeMode=NoResize, ShowInTaskbar=False, WindowBackdropType=None, ExtendsContentIntoTitleBar=True, Background=NeuBackgroundBrush
  - **Outer shell**: Border CornerRadius=14, BorderBrush=NeuInputFillBrush, BorderThickness=1, DropShadowEffect Opacity=0.18 BlurRadius=24
  - **Layout**: Grid with 3 rows (Auto header / * scrollable content / Auto footer)
  - **Header**: close button (DlgCloseButtonStyle, Dismiss24 icon) left, then SymbolIcon + title TextBlock (NeuJaugAccentBrush, Bold 18) + subtitle (NumeroXxx, secondary 12px) right
  - **Content**: ScrollViewer → 2×2 Grid (ColumnDefinitions: * / 12 / *; RowDefinitions: Auto/Auto) — 4 cards in two side-by-side pairs
  - **Cards**: Border Style=DlgCardStyle (Background=NeuBackgroundBrush, BorderBrush=NeuInputFillBrush, CornerRadius=12, Padding=20,18, Margin=0,0,0,12)
  - **Card header**: StackPanel Horizontal → DlgIconCircleStyle Border (28×28, CornerRadius=14, NeuInputFillBrush) with SymbolIcon (NeuAccentBrush, 14px) + TextBlock DlgCardTitleStyle (SemiBold 14)
  - **Field rows inside cards**: Grid with cols (*,1,*) — StackPanel left, Border(1px NeuInputFillBrush) divider, StackPanel right; label=DetailLabelStyle (10px SemiBold NeuTextSecondaryBrush), value=DetailValueStyle (13px NeuTextPrimaryBrush Wrap)
  - **Accent amounts**: DetailAccentValueStyle (15px SemiBold NeuAccentBrush); large numbers: FontSize=20 Bold NeuTextPrimaryBrush
  - **Prix pill**: Border NeuInputFillBrush CornerRadius=10 Padding=16,12; Grid(*,Auto) — label+small text left, large bold accent value right
  - **Footer**: Border BorderThickness=0,1,0,0 NeuInputFillBrush, Padding=22,14; StackPanel HorizontalAlignment=Right; secondary button (DlgSecondaryButtonStyle) + primary button (DlgPrimaryButtonStyle NeuAccentBrush→NeuAccentHoverBrush hover)
  - **Primary button content**: StackPanel Horizontal with SymbolIcon + TextBlock StringFormat='  {0}'
  - **Create/Edit dialogs**: same outer shell but content is ScrollViewer → StackPanel → 2×2 Grid of input cards; inputs use DlgInputBorderStyle+DlgInputTextBoxStyle, ComboBoxes for lookups, computed fields use DlgComputedBorderStyle (read-only), file picker uses DlgFilePickerButtonStyle; footer adds error TextBlock (NeuDangerBrush) + spinner/icon toggle on save button
  - **All labels/strings**: bound via LanguageManager.Instance indexer, keys prefixed by dialog name (e.g. NouveauVenteTitle, EditVenteClient, DetailVenteLabelNumero)
  - **Localization**: keys added to both Strings.resx (French) and Strings.ar.resx (Arabic); grouped by dialog with <!-- ── XxxDialog popup ── --> comment; inserted before the matching Achat block as anchor

## MVVM + Service Backend Pattern
### ViewModels
- **NouveauXxxViewModel**: ObservableObject, fields for all form inputs, NumeroXxx auto-generated as static GenerateNumero() → format "X-yyyy-MM-dd-HH-mm-ss", ObservableCollections for lookups, PaymentMethods IReadOnlyList, IAsyncRelayCommand SaveCommand, IRelayCommand BrowseReferenceFileCommand (OpenFileDialog), LoadLookupsAsync() fills collections, RecomputeTotal() on Quantite/PrixUnitaire/Remise change, SaveAsync() validates then calls service.CreateXxxAsync(dto), Saved bool flag.
- **EditXxxViewModel**: same fields + _xxxId, _originalQty, _originalAjoutePar, _originalDateCreation, _pendingClientId/_pendingProduitId stored from incoming dto; constructor pre-fills all fields; LoadLookupsAsync() restores FK selections without overwriting snapshotted price; SaveAsync() forwards AjoutePar+DateCreation from originals, sets ModifiePar via service.
- **Both VMs**: NullIfBlank() helper, GenerateNumero() static helper, PaymentMethods = ["TPE","Virement","Especes"].

### Dialog Code-Behind
- **Constructor**: InitializeComponent(), new XxxViewModel(dto), DataContext = ViewModel.
- **OnContentRendered**: await ViewModel.LoadLookupsAsync(); subscribe to SaveCommand.PropertyChanged to auto-close when !IsRunning && Saved.
- **OnCancelClick / OnCloseClick**: Close().
- **ShouldEdit** bool property on Detail dialogs; OnEditClick sets ShouldEdit=true then Close().

### Service Pattern (XxxService)
- **CreateXxxAsync**: stamps AjoutePar=App.CurrentUser?.UserId, DateCreation=DateTime.UtcNow before POST.
- **UpdateXxxAsync**: stamps ModifiePar=App.CurrentUser?.UserId, DateModification=DateTime.UtcNow before PUT.
- Edit ViewModel preserves AjoutePar+DateCreation from original DTO so the PUT never nulls them out.

### DTO Pattern (VenteDto example)
- Id, NumeroXxx, DateXxx (DateOnly), FK IDs + Nom strings, Quantite, PrixUnitaire, Remise, MontantTotal, PaymentMethod, Note, Reference, ReferenceFile, AjoutePar+AjouteParNom, DateCreation, ModifiePar+ModifieParNom, DateModification.