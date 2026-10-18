using Godot;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using WorldState.Domain.Data;
using WorldState.Domain.Map;
using WorldState.Domain.Models;
using WorldState.Domain.Scenarios;
using WorldState.Domain.ViewModels;

namespace WorldState.Desktop.Scenes;

public partial class CountrySelect : Control
{
    private CountryRegistry _registry = new();
    private CountrySelectionViewModel? _viewModel;
    private readonly ScenarioDefinition _scenario = ScenarioDefinition.CreateDefault();

    private LineEdit? _searchBox;
    private VBoxContainer? _countryListContainer;
    private Label? _countryNameLabel;
    private Label? _capitalLabel;
    private Label? _treasuryLabel;
    private Label? _populationLabel;
    private Label? _metricsLabel;
    private Label? _taxRatesLabel;
    private Label? _statusLabel;
    private Button? _startBtn;
    private Button? _backBtn;

    public override void _Ready()
    {
        _searchBox = GetNodeOrNull<LineEdit>("HBox/LeftPanel/SearchBox");
        _countryListContainer = GetNodeOrNull<VBoxContainer>("HBox/LeftPanel/Scroll/CountryList");

        _countryNameLabel = GetNodeOrNull<Label>("HBox/RightPanel/DetailsBox/CountryNameLabel");
        _capitalLabel = GetNodeOrNull<Label>("HBox/RightPanel/DetailsBox/CapitalLabel");
        _treasuryLabel = GetNodeOrNull<Label>("HBox/RightPanel/DetailsBox/TreasuryLabel");
        _populationLabel = GetNodeOrNull<Label>("HBox/RightPanel/DetailsBox/PopulationLabel");
        _metricsLabel = GetNodeOrNull<Label>("HBox/RightPanel/DetailsBox/MetricsLabel");
        _taxRatesLabel = GetNodeOrNull<Label>("HBox/RightPanel/DetailsBox/TaxRatesLabel");
        _statusLabel = GetNodeOrNull<Label>("HBox/RightPanel/DetailsBox/StatusLabel");

        _startBtn = GetNodeOrNull<Button>("HBox/RightPanel/ActionsBox/StartButton");
        _backBtn = GetNodeOrNull<Button>("HBox/RightPanel/ActionsBox/BackButton");

        InitializeRegistry();

        _viewModel = new CountrySelectionViewModel(_registry);
        _viewModel.OnStateChanged += UpdateUi;

        if (_searchBox != null)
        {
            _searchBox.TextChanged += (text) => _viewModel?.SetSearchQuery(text);
        }

        if (_startBtn != null)
        {
            _startBtn.Pressed += OnStartPressed;
        }

        if (_backBtn != null)
        {
            _backBtn.Pressed += OnBackPressed;
        }

        UpdateUi();
    }

    private void InitializeRegistry()
    {
        string contentDir = Path.Combine(Directory.GetCurrentDirectory(), "content", "countries");
        if (!Directory.Exists(contentDir))
        {
            contentDir = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "content", "countries");
        }

        if (Directory.Exists(contentDir))
        {
            foreach (var file in Directory.GetFiles(contentDir, "*.json"))
            {
                try
                {
                    string json = File.ReadAllText(file);
                    var def = JsonSerializer.Deserialize<CountryDefinition>(json);
                    if (def != null && !_registry.TryGet(def.Id, out _))
                    {
                        _registry.Register(def);
                    }
                }
                catch
                {
                    // Fallback to built-in list below
                }
            }
        }

        // Ensure baseline fallback definitions if files are not accessible in packaged run
        if (_registry.Count == 0)
        {
            RegisterFallbackCountries();
        }
    }

    private void RegisterFallbackCountries()
    {
        _registry.Register(new CountryDefinition("IND", "Republic of India", "New Delhi", true, 50_000_000_000m, 1_400_000_000L, 65.0, 55.0, 70.0, 0.20, 0.25, 0.15, new Dictionary<string, int>()));
        _registry.Register(new CountryDefinition("USA", "United States of America", "Washington, D.C.", true, 100_000_000_000m, 335_000_000L, 50.0, 75.0, 65.0, 0.22, 0.21, 0.0, new Dictionary<string, int>()));
        _registry.Register(new CountryDefinition("CHN", "People's Republic of China", "Beijing", true, 80_000_000_000m, 1_410_000_000L, 70.0, 60.0, 80.0, 0.25, 0.25, 0.13, new Dictionary<string, int>()));
        _registry.Register(new CountryDefinition("RUS", "Russian Federation", "Moscow", true, 40_000_000_000m, 144_000_000L, 58.0, 52.0, 60.0, 0.13, 0.20, 0.20, new Dictionary<string, int>()));
        _registry.Register(new CountryDefinition("GBR", "United Kingdom", "London", true, 35_000_000_000m, 67_000_000L, 48.0, 72.0, 68.0, 0.20, 0.25, 0.20, new Dictionary<string, int>()));
        _registry.Register(new CountryDefinition("PAK", "Islamic Republic of Pakistan", "Islamabad", true, 10_000_000_000m, 240_000_000L, 52.0, 42.0, 50.0, 0.15, 0.29, 0.17, new Dictionary<string, int>()));
    }

    private void UpdateUi()
    {
        if (_viewModel == null) return;

        // Rebuild country buttons list
        if (_countryListContainer != null)
        {
            foreach (var child in _countryListContainer.GetChildren())
            {
                child.QueueFree();
            }

            foreach (var country in _viewModel.FilteredCountries)
            {
                var btn = new Button
                {
                    Text = $"{country.Name} ({country.Id})",
                    CustomMinimumSize = new Vector2(0, 44)
                };

                string cId = country.Id;
                btn.Pressed += () => _viewModel.SelectCountry(cId);

                if (_viewModel.SelectedCountry?.Id == country.Id)
                {
                    btn.Modulate = new Color(0.23f, 0.51f, 0.96f); // Highlight selected
                }

                _countryListContainer.AddChild(btn);
            }
        }

        // Update inspection panel
        var sel = _viewModel.SelectedCountry;
        if (sel != null)
        {
            if (_countryNameLabel != null) _countryNameLabel.Text = $"{sel.Name} [{sel.Id}]";
            if (_capitalLabel != null) _capitalLabel.Text = $"Capital: {sel.Capital}";
            if (_treasuryLabel != null) _treasuryLabel.Text = $"Treasury: {HudViewModel.FormatCurrency(sel.StartingTreasury)}";
            if (_populationLabel != null) _populationLabel.Text = $"Population: {HudViewModel.FormatNumber(sel.StartingPopulation)}";
            if (_metricsLabel != null) _metricsLabel.Text = $"Approval: {sel.StartingApproval:F1}%  •  Living Std: {sel.StartingQualityOfLiving:F1}%  •  Stability: {sel.StartingStability:F1}%";
            if (_taxRatesLabel != null) _taxRatesLabel.Text = $"Income Tax: {sel.IncomeTaxRate * 100:F0}%  •  Corporate: {sel.CorporateTaxRate * 100:F0}%  •  VAT: {sel.VatRate * 100:F0}%";
            if (_statusLabel != null)
            {
                _statusLabel.Text = sel.IsPlayable ? "STATUS: SOVEREIGN NATION READY TO LEAD" : "STATUS: NON-PLAYABLE NATION";
                _statusLabel.Modulate = sel.IsPlayable ? new Color(0.06f, 0.72f, 0.50f) : new Color(0.85f, 0.2f, 0.2f);
            }
        }

        if (_startBtn != null)
        {
            _startBtn.Disabled = !_viewModel.CanStartGame;
        }
    }

    private void OnStartPressed()
    {
        if (_viewModel?.SelectedCountry == null || !_viewModel.CanStartGame) return;

        var config = new NewGameConfig(
            ScenarioId: _scenario.Id,
            PlayerCountryId: _viewModel.SelectedCountry.Id,
            Seed: 42,
            Difficulty: "Standard"
        );

        var session = GameInitializer.InitializeNewGame(config, _registry, _scenario);
        GameShell.ActiveSession = session;

        GetTree().ChangeSceneToFile("res://scenes/GameShell.tscn");
    }

    private void OnBackPressed()
    {
        GetTree().ChangeSceneToFile("res://scenes/MainMenu.tscn");
    }
}
