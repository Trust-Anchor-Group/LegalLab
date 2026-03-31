# LegalLabMaui – Port Plan

Full port of the LegalLab WPF desktop app to .NET MAUI 9 targeting **Windows** and **Android**.

---

## Architecture Decisions

| WPF | MAUI |
|-----|------|
| `MainWindow` static helpers | `AppService` static class |
| `Dispatcher.BeginInvoke` | `MainThread.InvokeOnMainThreadAsync` |
| `ProtectedData` (DPAPI) | `SecureStorage` |
| `System.Windows.Media.Color` | `Microsoft.Maui.Graphics.Color` |
| `System.Windows.Clipboard` | `Clipboard.SetTextAsync()` |
| `AvalonEdit` (XML/Markdown editor) | MAUI `Editor` control |
| `ListView` + custom item sink | `ObservableCollection<T>` |
| `Window.ShowDialog()` | Shell navigation routes + `TaskCompletionSource<bool>` |
| WPF PasswordBox events | `Entry` with `IsPassword="True"` + `TextChanged` handler |
| `Process.Start(url)` | `AppService.OpenUrl(new Uri(url))` |
| `OpenFileDialog` | `FilePicker.PickAsync()` |
| `SaveFileDialog` | `FileSystem.AppDataDirectory` + file write |
| WPF `BitmapImage` | `byte[]` glyph + MAUI `ImageSource` |
| WPF `DataGrid`/`ListView` | `CollectionView` with `ItemTemplate` |
| Token reports (WPF XAML rendering) | Markdown-based reports (`GetReportMarkdown()`) |
| Dialog `DialogResult` | `TaskCompletionSource<bool>.WaitForResult()` |

---

## Progress

### ✅ Phase 1 – Project Infrastructure (complete)

| File | Status |
|------|--------|
| `LegalLabMaui.csproj` | ✅ |
| `MauiProgram.cs` | ✅ |
| `App.xaml` / `App.xaml.cs` | ✅ |
| `AppShell.xaml` / `AppShell.xaml.cs` | ✅ |
| `AppService.cs` | ✅ |
| `Platforms/Android/` | ✅ |
| `Platforms/Windows/` | ✅ |
| `Resources/Styles/Colors.xaml` | ✅ |
| `Resources/Styles/Styles.xaml` | ✅ |

### ✅ Phase 2 – Core Model Infrastructure (complete)

| File | Status |
|------|--------|
| `Models/IModel.cs` | ✅ |
| `Models/IProperty.cs` | ✅ |
| `Models/IPersistedProperty.cs` | ✅ |
| `Models/IDelayedAction.cs` | ✅ |
| `Models/Model.cs` | ✅ |
| `Models/PersistedModel.cs` | ✅ |
| `Models/Property.cs` | ✅ |
| `Models/PersistedProperty.cs` | ✅ |
| `Models/DelayedActionProperty.cs` | ✅ |
| `Models/DelayedActions.cs` | ✅ |
| `Models/Command.cs` | ✅ |
| `Models/ParametrizedCommand.cs` | ✅ |
| `Models/ReverseOrder.cs` | ✅ |
| `Models/Items/ColorableItem.cs` | ✅ |
| `Models/Items/SelectableItem.cs` | ✅ |
| `Models/Items/INamedItem.cs` | ✅ |
| `Models/Items/OrderedItem.cs` | ✅ |
| `Models/Events/LogItem.cs` | ✅ |
| `Models/Events/ObservableEventSink.cs` | ✅ |
| `Models/Network/Sniffer/SniffItem.cs` | ✅ |
| `Models/Network/Sniffer/ObservableSniffer.cs` | ✅ |
| `Models/Window/WindowSizeModel.cs` | ✅ |

### ✅ Phase 3 – Business Models (complete)

#### Network
| File | Status |
|------|--------|
| `Models/Network/ConnectionSensitiveModel.cs` | ✅ |
| `Models/Network/NetworkModel.cs` | ✅ |

#### Legal – Items
| File | Status |
|------|--------|
| `Models/Legal/Items/GenInfo.cs` | ✅ |
| `Models/Legal/Items/AttachmentInfo.cs` | ✅ |
| `Models/Legal/Items/ClientSignatureInfo.cs` | ✅ |
| `Models/Legal/Items/ServerSignatureInfo.cs` | ✅ |
| `Models/Legal/Items/PartInfo.cs` | ✅ |
| `Models/Legal/Items/RoleInfo.cs` | ✅ |
| `Models/Legal/Items/ParameterInfo.cs` | ✅ |

#### Legal – Parameters
| File | Status |
|------|--------|
| `Models/Legal/Items/Parameters/BooleanParameterInfo.cs` | ✅ |
| `Models/Legal/Items/Parameters/NumericalParameterInfo.cs` | ✅ |
| `Models/Legal/Items/Parameters/StringParameterInfo.cs` | ✅ |
| `Models/Legal/Items/Parameters/DateParameterInfo.cs` | ✅ |
| `Models/Legal/Items/Parameters/DateTimeParameterInfo.cs` | ✅ |
| `Models/Legal/Items/Parameters/TimeParameterInfo.cs` | ✅ |
| `Models/Legal/Items/Parameters/DurationParameterInfo.cs` | ✅ |
| `Models/Legal/Items/Parameters/CalcParameterInfo.cs` | ✅ |
| `Models/Legal/Items/Parameters/GeoParameterInfo.cs` | ✅ |
| `Models/Legal/Items/Parameters/RangedParameterInfo.cs` | ✅ |
| `Models/Legal/Items/Parameters/RoleParameterInfo.cs` | ✅ |
| `Models/Legal/Items/Parameters/ContractReferenceParameterInfo.cs` | ✅ |
| `Models/Legal/Items/Parameters/RoleReferenceParameterInfo.cs` | ✅ |

#### Legal – Models
| File | Status |
|------|--------|
| `Models/Legal/ContractReferenceModel.cs` | ✅ |
| `Models/Legal/TemplateReferenceModel.cs` | ✅ |
| `Models/Legal/ContractOption.cs` | ✅ |
| `Models/Legal/ContractParametersModel.cs` | ✅ |
| `Models/Legal/IdentityWrapper.cs` | ✅ |
| `Models/Legal/ContractModel.cs` | ✅ |
| `Models/Legal/LegalModel.cs` | ✅ |

#### Wallet
| File | Status |
|------|--------|
| `Models/Wallet/AccountEventWrapper.cs` | ✅ |
| `Models/Wallet/WalletModel.cs` | ✅ |

#### Tokens
| File | Status |
|------|--------|
| `Models/Tokens/TokenModel.cs` | ✅ |
| `Models/Tokens/TokensModel.cs` | ✅ |
| `Models/Tokens/TokenDetail.cs` | ✅ |
| `Models/Tokens/Details/TokenEventDetail.cs` | ✅ |
| `Models/Tokens/Details/CreatedDetail.cs` | ✅ |
| `Models/Tokens/Details/DestroyedDetail.cs` | ✅ |
| `Models/Tokens/Details/ValueDetail.cs` | ✅ |
| `Models/Tokens/Details/OwnershipDetail.cs` | ✅ |
| `Models/Tokens/Details/TransferredDetail.cs` | ✅ |
| `Models/Tokens/Details/DonatedDetail.cs` | ✅ |
| `Models/Tokens/Details/KilledDetail.cs` | ✅ |
| `Models/Tokens/Details/NoteDetail.cs` | ✅ |
| `Models/Tokens/Details/NoteTextDetail.cs` | ✅ |
| `Models/Tokens/Details/NoteXmlDetail.cs` | ✅ |
| `Models/Tokens/Details/ExternalNoteDetail.cs` | ✅ |
| `Models/Tokens/Details/ExternalNoteTextDetail.cs` | ✅ |
| `Models/Tokens/Details/ExternalNoteXmlDetail.cs` | ✅ |
| `Models/Tokens/Reports/TokenReport.cs` | ✅ |
| `Models/Tokens/Reports/TokenEmbeddedLayout.cs` | ✅ |
| `Models/Tokens/Reports/TokenHistoryReport.cs` | ✅ |
| `Models/Tokens/Reports/TokenPresentReport.cs` | ✅ |
| `Models/Tokens/Reports/TokenProfilingReport.cs` | ✅ |
| `Models/Tokens/Reports/TokenStateDiagramReport.cs` | ✅ |

#### Design
| File | Status |
|------|--------|
| `Models/Design/IPartsModel.cs` | ✅ |
| `Models/Design/ITranslatable.cs` | ✅ |
| `Models/Design/Translator.cs` | ✅ |
| `Models/Design/DesignModel.cs` | ✅ |

#### Other Models
| File | Status |
|------|--------|
| `Models/Script/PrintOutput.cs` | ✅ |
| `Models/Script/ScriptModel.cs` | ✅ |
| `Models/XmlEditor/XmlEditorModel.cs` | ✅ |

#### Standards
| File | Status |
|------|--------|
| `Models/Standards/Iso_3166_1.cs` | ✅ |
| `Models/Standards/Iso_5218.cs` | ✅ |
| `Models/Standards/Iso__639_1.cs` | ✅ |

---

### ✅ Phase 4 – Views / Pages (complete)

#### Main Tabs (10 pages)
| File | Status |
|------|--------|
| `Views/NetworkPage.xaml` + `.xaml.cs` | ✅ |
| `Views/LegalIdPage.xaml` + `.xaml.cs` | ✅ |
| `Views/WalletPage.xaml` + `.xaml.cs` | ✅ |
| `Views/TokensPage.xaml` + `.xaml.cs` | ✅ |
| `Views/DesignPage.xaml` + `.xaml.cs` | ✅ |
| `Views/ContractsPage.xaml` + `.xaml.cs` | ✅ |
| `Views/EventsPage.xaml` + `.xaml.cs` | ✅ |
| `Views/ScriptPage.xaml` + `.xaml.cs` | ✅ |
| `Views/XmlEditorPage.xaml` + `.xaml.cs` | ✅ |
| `Views/ReportPage.xaml` + `.xaml.cs` | ✅ |

#### Dialog Pages (7 pages)
| File | Status |
|------|--------|
| `Views/Dialogs/PromptPage.xaml` + `.xaml.cs` | ✅ |
| `Views/Dialogs/AddLanguagePage.xaml` + `.xaml.cs` | ✅ |
| `Views/Dialogs/AddXmlNotePage.xaml` + `.xaml.cs` | ✅ |
| `Views/Dialogs/BuyEDalerPage.xaml` + `.xaml.cs` | ✅ |
| `Views/Dialogs/SellEDalerPage.xaml` + `.xaml.cs` | ✅ |
| `Views/Dialogs/TransferEDalerPage.xaml` + `.xaml.cs` | ✅ |
| `Views/Dialogs/ParametersPage.xaml` + `.xaml.cs` | ✅ |

#### Dialog Models
| File | Status |
|------|--------|
| `Views/Dialogs/PromptModel.cs` | ✅ |
| `Views/Dialogs/AddLanguageModel.cs` | ✅ |
| `Views/Dialogs/AddXmlNoteModel.cs` | ✅ |
| `Views/Dialogs/BuyEDalerModel.cs` | ✅ |
| `Views/Dialogs/SellEDalerModel.cs` | ✅ |
| `Views/Dialogs/TransferEDalerModel.cs` | ✅ |
| `Views/Dialogs/ServiceProviderModel.cs` | ✅ |

---

### ✅ Phase 5 – Converters, Extensions, Resources (complete)

#### Converters (15 files)
| File | Status |
|------|--------|
| `Converters/BooleanToVisibility.cs` | ✅ |
| `Converters/BooleanToYesNo.cs` | ✅ |
| `Converters/ContractPartsToString.cs` | ✅ |
| `Converters/ContractVisibilityToString.cs` | ✅ |
| `Converters/DateTimeToString.cs` | ✅ |
| `Converters/DateTimeToXmlString.cs` | ✅ |
| `Converters/DateToString.cs` | ✅ |
| `Converters/DateToXmlString.cs` | ✅ |
| `Converters/DurationToString.cs` | ✅ |
| `Converters/DurationToXmlString.cs` | ✅ |
| `Converters/GeoPositionToGpsString.cs` | ✅ |
| `Converters/MoneyToString.cs` | ✅ |
| `Converters/Not.cs` | ✅ |
| `Converters/NotBooleanToVisibility.cs` | ✅ |
| `Converters/ProtectionLevelToColor.cs` | ✅ (replaces WPF `ProtectionLevelToBrush`) |
| `Converters/ProtectionLevelToString.cs` | ✅ |
| `Converters/TimeSpanToXmlString.cs` | ✅ |
| `Converters/ToBase64.cs` | ✅ |
| `Converters/ToString.cs` | ✅ |
| `Converters/VisibleIfNotNull.cs` | ✅ |

#### Extensions (4 files)
| File | Status |
|------|--------|
| `Extensions/ContractExtensions.cs` | ✅ |
| `Extensions/ScriptExtensions.cs` | ✅ |
| `Extensions/SimpleExtensions.cs` | ✅ |
| `Extensions/StandardExtensions.cs` | ✅ |

---

## Summary

| Phase | Status | Files done |
|-------|--------|------------|
| 1 – Infrastructure | ✅ Complete | 9 |
| 2 – Core models | ✅ Complete | 22 |
| 3 – Business models | ✅ Complete | ~70 |
| 4 – Views/pages | ✅ Complete | 34 |
| 5 – Converters/extensions | ✅ Complete | 24 |
| **Total** | **✅ Complete** | **~160** |

## Key Architectural Differences from WPF

1. **Dialog flow**: WPF used `Window.ShowDialog()` returning `bool?`. MAUI uses Shell navigation routes + `TaskCompletionSource<bool>` pattern. Callers navigate to the dialog route and then `await model.WaitForResult()`.

2. **Token reports**: WPF rendered reports as WPF XAML trees. MAUI reports use `GetReportMarkdown()` returning Markdown strings that the ReportPage renders.

3. **File I/O**: `OpenFileDialog`/`SaveFileDialog` replaced by `FilePicker.PickAsync()` and `FileSystem.AppDataDirectory` writes.

4. **Text editors**: AvalonEdit `TextEditor` controls replaced by MAUI `Editor` bound to `string` properties on the models.

5. **Clipboard**: `Clipboard.SetText()` → `Clipboard.SetTextAsync()`.

6. **Browser**: `Process.Start(url)` → `AppService.OpenUrl(new Uri(url))` → `Browser.OpenAsync()`.
