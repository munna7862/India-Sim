import React, { useState } from 'react';
import { GameSession } from '../../domain/models/GameSession';
import { SimulationEngine } from '../../domain/services/SimulationEngine';
import {
  SendDiplomaticGiftCommand,
  SignNonAggressionPactCommand,
  SignTradeContractCommand,
  CancelTradeContractCommand,
  DeclareWarCommand,
  SignPeaceTreatyCommand,
  DispatchCovertOperationCommand,
} from '../../domain/commands/DiplomacyMilitaryCommands';
import { DiplomaticGiftTier, TradeContract } from '../../domain/models/Diplomacy';
import { CovertOperationType } from '../../domain/models/types';
import {
  Globe2,
  Handshake,
  DollarSign,
  Shield,
  ShieldAlert,
  Swords,
  Scroll,
  Eye,
  AlertTriangle,
  X,
  Check,
  Send,
  Trash2,
} from 'lucide-react';

interface DiplomacyPanelProps {
  session: GameSession;
  engine: SimulationEngine;
  onClose: () => void;
  onUpdate: () => void;
  initialSelectedCountryId?: string;
}

export const DiplomacyPanel: React.FC<DiplomacyPanelProps> = ({
  session,
  engine,
  onClose,
  onUpdate,
  initialSelectedCountryId,
}) => {
  const player = session.getPlayerCountry();
  const allCountries = Object.values(session.countries);
  const foreignCountries = allCountries.filter((c) => c.id !== session.playerCountryId);

  const [selectedTargetId, setSelectedTargetId] = useState<string>(
    initialSelectedCountryId || foreignCountries[0]?.id || ''
  );
  const [activeTab, setActiveTab] = useState<'relations' | 'trade' | 'covert'>('relations');
  const [feedback, setFeedback] = useState<{ msg: string; isError: boolean } | null>(null);

  // Trade form state
  const [tradeResource, setTradeResource] = useState<string>('Oil');
  const [tradeAmount, setTradeAmount] = useState<number>(100);
  const [isExport, setIsExport] = useState<boolean>(false);

  // Covert state
  const [covertType, setCovertType] = useState<CovertOperationType>(CovertOperationType.GatherIntelligence);

  if (!player) return null;

  const targetCountry = session.countries[selectedTargetId];
  const stance = targetCountry ? player.getRelationsWith(targetCountry.id) : null;

  const showMsg = (msg: string, isError: boolean = false) => {
    setFeedback({ msg, isError });
    setTimeout(() => setFeedback(null), 3500);
  };

  const handleSendGift = (tier: DiplomaticGiftTier) => {
    if (!targetCountry) return;
    const cmd = new SendDiplomaticGiftCommand(player.id, targetCountry.id, tier);
    const res = engine.dispatch(cmd);
    if (res.isSuccess) {
      showMsg(`Dispatched ${tier} diplomatic package to ${targetCountry.name}`);
      onUpdate();
    } else {
      showMsg(res.errorMessage || 'Gift dispatch failed', true);
    }
  };

  const handleSignNap = () => {
    if (!targetCountry) return;
    const cmd = new SignNonAggressionPactCommand(player.id, targetCountry.id);
    const res = engine.dispatch(cmd);
    if (res.isSuccess) {
      showMsg(`Signed Non-Aggression Pact with ${targetCountry.name}!`);
      onUpdate();
    } else {
      showMsg(res.errorMessage || 'Failed to sign Non-Aggression Pact', true);
    }
  };

  const handleCreateTradeContract = () => {
    if (!targetCountry) return;
    const monthlyPayment = tradeAmount * 250_000;
    const cmd = isExport
      ? new SignTradeContractCommand(player.id, targetCountry.id, tradeResource, tradeAmount, monthlyPayment, 24)
      : new SignTradeContractCommand(targetCountry.id, player.id, tradeResource, tradeAmount, monthlyPayment, 24);

    const res = engine.dispatch(cmd);
    if (res.isSuccess) {
      showMsg(`Trade contract established with ${targetCountry.name}`);
      onUpdate();
    } else {
      showMsg(res.errorMessage || 'Failed to create trade contract', true);
    }
  };

  const handleCancelTrade = (contractId: string) => {
    const cmd = new CancelTradeContractCommand(player.id, contractId);
    const res = engine.dispatch(cmd);
    if (res.isSuccess) {
      showMsg('Trade contract terminated.');
      onUpdate();
    } else {
      showMsg(res.errorMessage || 'Failed to cancel contract', true);
    }
  };

  const handleDeclareWar = () => {
    if (!targetCountry) return;
    if (confirm(`WAR DECLARATION WARNING: Are you certain you want to declare war on ${targetCountry.name}? This will rupture all diplomatic ties.`)) {
      const cmd = new DeclareWarCommand(player.id, targetCountry.id);
      const res = engine.dispatch(cmd);
      if (res.isSuccess) {
        showMsg(`State of War declared against ${targetCountry.name}!`, true);
        onUpdate();
      } else {
        showMsg(res.errorMessage || 'War declaration failed', true);
      }
    }
  };

  const handleSignPeace = () => {
    if (!targetCountry) return;
    const cmd = new SignPeaceTreatyCommand(player.id, targetCountry.id);
    const res = engine.dispatch(cmd);
    if (res.isSuccess) {
      showMsg(`Peace Treaty ratified with ${targetCountry.name}. Hostilities ceased.`);
      onUpdate();
    } else {
      showMsg(res.errorMessage || 'Failed to sign Peace Treaty', true);
    }
  };

  const handleDispatchCovert = () => {
    if (!targetCountry) return;
    const cmd = new DispatchCovertOperationCommand(player.id, targetCountry.id, covertType);
    const res = engine.dispatch(cmd);
    if (res.isSuccess) {
      showMsg(`Intelligence operative dispatched for ${covertType}`);
      onUpdate();
    } else {
      showMsg(res.errorMessage || 'Covert operation failed to dispatch', true);
    }
  };

  const getRelationsColor = (rel: number) => {
    if (rel >= 60) return 'text-emerald-400';
    if (rel >= 20) return 'text-green-400';
    if (rel >= -20) return 'text-slate-300';
    if (rel >= -60) return 'text-amber-400';
    return 'text-rose-400';
  };

  const getStanceBadge = () => {
    if (!stance) return null;
    if (stance.isAtWar) {
      return (
        <span className="badge badge-danger">
          <Swords className="w-3 h-3 inline mr-1" /> AT WAR
        </span>
      );
    }
    if (stance.hasNonAggressionPact) {
      return (
        <span className="badge badge-success">
          <Shield className="w-3 h-3 inline mr-1" /> Non-Aggression Pact
        </span>
      );
    }
    if (stance.relations >= 50) return <span className="badge badge-success">Allied Stance</span>;
    if (stance.relations >= 20) return <span className="badge badge-info">Friendly</span>;
    if (stance.relations >= -20) return <span className="badge badge-neutral">Neutral</span>;
    return <span className="badge badge-warning">Strained</span>;
  };

  // Find contracts involving the player and selected target
  const activeContracts: TradeContract[] = session.tradeContracts.filter(
    (c: TradeContract) =>
      (c.importerCountryId === player.id && c.exporterCountryId === selectedTargetId) ||
      (c.exporterCountryId === player.id && c.importerCountryId === selectedTargetId)
  );

  return (
    <div className="panel-overlay" onClick={onClose}>
      <div
        className="glass-card panel-container max-w-5xl w-full max-h-[90vh] flex flex-col overflow-hidden"
        onClick={(e) => e.stopPropagation()}
      >
        {/* Header */}
        <div className="p-4 border-b border-slate-700/60 flex items-center justify-between bg-slate-900/60">
          <div className="flex items-center gap-3">
            <Globe2 className="w-6 h-6 text-indigo-400" />
            <div>
              <h2 className="text-xl font-bold text-white tracking-wide">Ministry of External Affairs</h2>
              <p className="text-xs text-slate-400">Global Geopolitical Bilateral Affairs & Commercial Treaties</p>
            </div>
          </div>
          <button
            onClick={onClose}
            className="p-1.5 text-slate-400 hover:text-white rounded-lg hover:bg-slate-800/80 transition-colors"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Feedback Alert */}
        {feedback && (
          <div
            className={`mx-4 mt-3 p-3 rounded-lg border text-sm flex items-center gap-2 ${
              feedback.isError
                ? 'bg-rose-950/60 border-rose-500/50 text-rose-200'
                : 'bg-emerald-950/60 border-emerald-500/50 text-emerald-200'
            }`}
          >
            {feedback.isError ? <AlertTriangle className="w-4 h-4 shrink-0" /> : <Check className="w-4 h-4 shrink-0" />}
            <span>{feedback.msg}</span>
          </div>
        )}

        <div className="flex-1 flex overflow-hidden">
          {/* Country Selector Sidebar */}
          <div className="w-64 border-r border-slate-800/80 bg-slate-950/40 p-3 overflow-y-auto flex flex-col gap-1.5">
            <div className="text-xs uppercase font-mono text-slate-400 px-2 py-1">Foreign Sovereign States</div>
            {foreignCountries.map((c) => {
              const rel = player.getRelationsWith(c.id);
              const isSelected = c.id === selectedTargetId;
              return (
                <button
                  key={c.id}
                  onClick={() => setSelectedTargetId(c.id)}
                  className={`w-full text-left p-2.5 rounded-lg border transition-all flex items-center justify-between ${
                    isSelected
                      ? 'bg-indigo-600/30 border-indigo-500/60 text-white shadow-sm shadow-indigo-500/20'
                      : 'bg-slate-900/40 border-slate-800/60 text-slate-300 hover:bg-slate-800/50'
                  }`}
                >
                  <div className="flex items-center gap-2">
                    <span className="font-mono text-xs px-1.5 py-0.5 rounded bg-slate-800 text-slate-300 font-bold">
                      {c.id}
                    </span>
                    <span className="text-sm font-semibold truncate">{c.name}</span>
                  </div>
                  <div className="flex items-center gap-1.5">
                    {rel.isAtWar && <Swords className="w-3.5 h-3.5 text-rose-400 shrink-0" />}
                    <span className={`text-xs font-mono font-bold ${getRelationsColor(rel.relations)}`}>
                      {rel.relations > 0 ? `+${rel.relations.toFixed(0)}` : rel.relations.toFixed(0)}
                    </span>
                  </div>
                </button>
              );
            })}
          </div>

          {/* Target Country Work Area */}
          {targetCountry && stance ? (
            <div className="flex-1 flex flex-col overflow-y-auto p-5">
              {/* Target Country Header Card */}
              <div className="glass-panel p-4 mb-4 flex flex-col md:flex-row md:items-center justify-between gap-4">
                <div>
                  <div className="flex items-center gap-3">
                    <h3 className="text-2xl font-bold text-white">{targetCountry.name}</h3>
                    <span className="font-mono text-xs px-2 py-0.5 rounded bg-slate-800 text-indigo-300 font-bold">
                      {targetCountry.id}
                    </span>
                    {getStanceBadge()}
                  </div>
                  <p className="text-xs text-slate-400 mt-1">
                    GDP: ${(targetCountry.gdp / 1_000_000_000).toFixed(1)}B | Population: {(targetCountry.population / 1_000_000).toFixed(1)}M
                  </p>
                </div>

                {/* Bilateral Opinion Gauge */}
                <div className="flex flex-col items-end">
                  <div className="text-xs text-slate-400 mb-1">Bilateral Opinion</div>
                  <div className="flex items-center gap-2">
                    <div className="w-36 h-2.5 bg-slate-800 rounded-full overflow-hidden border border-slate-700">
                      <div
                        className={`h-full transition-all duration-500 ${
                          stance.relations >= 0 ? 'bg-emerald-500' : 'bg-rose-500'
                        }`}
                        style={{
                          width: `${Math.abs(stance.relations)}%`,
                          marginLeft: stance.relations < 0 ? '0%' : '50%',
                        }}
                      />
                    </div>
                    <span className={`font-mono text-sm font-bold ${getRelationsColor(stance.relations)}`}>
                      {stance.relations > 0 ? `+${stance.relations.toFixed(0)}` : stance.relations.toFixed(0)}
                    </span>
                  </div>
                </div>
              </div>

              {/* Sub-tabs */}
              <div className="flex items-center gap-2 border-b border-slate-800/80 mb-4 pb-2">
                <button
                  onClick={() => setActiveTab('relations')}
                  className={`px-3 py-1.5 rounded-lg text-sm font-medium transition-colors flex items-center gap-2 ${
                    activeTab === 'relations'
                      ? 'bg-indigo-600 text-white'
                      : 'text-slate-400 hover:text-white hover:bg-slate-800/50'
                  }`}
                >
                  <Handshake className="w-4 h-4" /> Diplomatic Envoys & Pacts
                </button>
                <button
                  onClick={() => setActiveTab('trade')}
                  className={`px-3 py-1.5 rounded-lg text-sm font-medium transition-colors flex items-center gap-2 ${
                    activeTab === 'trade'
                      ? 'bg-indigo-600 text-white'
                      : 'text-slate-400 hover:text-white hover:bg-slate-800/50'
                  }`}
                >
                  <Scroll className="w-4 h-4" /> Commercial Trade Agreements ({activeContracts.length})
                </button>
                <button
                  onClick={() => setActiveTab('covert')}
                  className={`px-3 py-1.5 rounded-lg text-sm font-medium transition-colors flex items-center gap-2 ${
                    activeTab === 'covert'
                      ? 'bg-indigo-600 text-white'
                      : 'text-slate-400 hover:text-white hover:bg-slate-800/50'
                  }`}
                >
                  <Eye className="w-4 h-4" /> Covert Intelligence
                </button>
              </div>

              {/* TAB 1: RELATIONS & PACTS */}
              {activeTab === 'relations' && (
                <div className="space-y-4">
                  {/* Foreign Aid Packages */}
                  <div className="glass-panel p-4">
                    <h4 className="text-sm font-bold text-slate-200 uppercase tracking-wider mb-2 flex items-center gap-2">
                      <DollarSign className="w-4 h-4 text-emerald-400" /> Foreign Development Aid Packages
                    </h4>
                    <p className="text-xs text-slate-400 mb-3">
                      Deliver official development assistance to boost bilateral goodwill (+5 to +25 relations, 30-day cooldown).
                    </p>
                    <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
                      <button
                        onClick={() => handleSendGift(DiplomaticGiftTier.Small)}
                        disabled={stance.isAtWar || player.treasury < 250_000_000}
                        className="p-3 rounded-lg border border-slate-700/80 bg-slate-900/60 hover:bg-slate-800 text-left transition-all disabled:opacity-50"
                      >
                        <div className="text-xs text-emerald-400 font-bold mb-1">Small Package</div>
                        <div className="text-base font-bold text-white mb-0.5">$250M</div>
                        <div className="text-xs text-slate-300">+5 Relations</div>
                      </button>
                      <button
                        onClick={() => handleSendGift(DiplomaticGiftTier.Medium)}
                        disabled={stance.isAtWar || player.treasury < 1_000_000_000}
                        className="p-3 rounded-lg border border-slate-700/80 bg-slate-900/60 hover:bg-slate-800 text-left transition-all disabled:opacity-50"
                      >
                        <div className="text-xs text-indigo-400 font-bold mb-1">Medium Package</div>
                        <div className="text-base font-bold text-white mb-0.5">$1.0 Billion</div>
                        <div className="text-xs text-slate-300">+12 Relations</div>
                      </button>
                      <button
                        onClick={() => handleSendGift(DiplomaticGiftTier.Large)}
                        disabled={stance.isAtWar || player.treasury < 5_000_000_000}
                        className="p-3 rounded-lg border border-slate-700/80 bg-slate-900/60 hover:bg-slate-800 text-left transition-all disabled:opacity-50"
                      >
                        <div className="text-xs text-purple-400 font-bold mb-1">Major Bilateral Grant</div>
                        <div className="text-base font-bold text-white mb-0.5">$5.0 Billion</div>
                        <div className="text-xs text-slate-300">+25 Relations</div>
                      </button>
                    </div>
                  </div>

                  {/* Non-Aggression Pact & Treaties */}
                  <div className="glass-panel p-4">
                    <h4 className="text-sm font-bold text-slate-200 uppercase tracking-wider mb-2 flex items-center gap-2">
                      <Shield className="w-4 h-4 text-cyan-400" /> Non-Aggression & Defense Accords
                    </h4>
                    <p className="text-xs text-slate-400 mb-3">
                      A mutual 365-day Non-Aggression Pact prevents surprise conflicts and increases trading affinity.
                      Requires relations ≥ +20 and $500M administration fee.
                    </p>
                    <div className="flex items-center gap-3">
                      {stance.hasNonAggressionPact ? (
                        <div className="text-sm text-emerald-300 flex items-center gap-2">
                          <Check className="w-4 h-4 text-emerald-400" /> Non-Aggression Pact is ratified and in active effect.
                        </div>
                      ) : (
                        <button
                          onClick={handleSignNap}
                          disabled={stance.isAtWar || stance.relations < 20 || player.treasury < 500_000_000}
                          className="btn btn-primary text-xs flex items-center gap-2 disabled:opacity-50"
                        >
                          <Shield className="w-4 h-4" /> Ratify Non-Aggression Pact ($500M)
                        </button>
                      )}
                    </div>
                  </div>

                  {/* Conflict & State of War */}
                  <div className="glass-panel p-4 border border-rose-900/40 bg-rose-950/10">
                    <h4 className="text-sm font-bold text-rose-300 uppercase tracking-wider mb-2 flex items-center gap-2">
                      <ShieldAlert className="w-4 h-4 text-rose-400" /> Geopolitical Conflict & Sovereignty
                    </h4>
                    {stance.isAtWar ? (
                      <div className="flex items-center justify-between">
                        <div>
                          <div className="text-sm font-bold text-rose-200">Active State of War</div>
                          <div className="text-xs text-rose-400">Armies are actively clashing in combat theaters.</div>
                        </div>
                        <button
                          onClick={handleSignPeace}
                          className="btn btn-primary bg-emerald-600 hover:bg-emerald-500 text-xs flex items-center gap-2"
                        >
                          <Handshake className="w-4 h-4" /> Negotiate Peace Accord
                        </button>
                      </div>
                    ) : (
                      <div className="flex items-center justify-between">
                        <div className="text-xs text-slate-400">
                          Declaring war initiates active combat theaters. Non-aggression treaties will be violated.
                        </div>
                        <button
                          onClick={handleDeclareWar}
                          className="btn btn-danger text-xs flex items-center gap-2"
                        >
                          <Swords className="w-4 h-4" /> Declare State of War
                        </button>
                      </div>
                    )}
                  </div>
                </div>
              )}

              {/* TAB 2: TRADE AGREEMENTS */}
              {activeTab === 'trade' && (
                <div className="space-y-4">
                  {/* Create Contract Form */}
                  <div className="glass-panel p-4">
                    <h4 className="text-sm font-bold text-slate-200 uppercase tracking-wider mb-2 flex items-center gap-2">
                      <Scroll className="w-4 h-4 text-amber-400" /> Draft Bilateral Trade Treaty
                    </h4>
                    <div className="grid grid-cols-1 md:grid-cols-4 gap-3 items-end">
                      <div>
                        <label className="text-xs text-slate-400 block mb-1">Commodity</label>
                        <select
                          value={tradeResource}
                          onChange={(e) => setTradeResource(e.target.value)}
                          className="w-full bg-slate-900 border border-slate-700 rounded-lg p-2 text-sm text-white focus:outline-none focus:border-indigo-500"
                        >
                          <option value="Oil">Oil (Crude)</option>
                          <option value="Energy">Energy (MWh)</option>
                          <option value="Iron">Iron Ore</option>
                          <option value="Uranium">Uranium</option>
                          <option value="Food">Food Surplus</option>
                          <option value="ConsumerGoods">Consumer Goods</option>
                        </select>
                      </div>
                      <div>
                        <label className="text-xs text-slate-400 block mb-1">Monthly Flow</label>
                        <select
                          value={isExport ? 'export' : 'import'}
                          onChange={(e) => setIsExport(e.target.value === 'export')}
                          className="w-full bg-slate-900 border border-slate-700 rounded-lg p-2 text-sm text-white focus:outline-none focus:border-indigo-500"
                        >
                          <option value="import">Import (Pay Money, Receive Commodity)</option>
                          <option value="export">Export (Ship Commodity, Receive Money)</option>
                        </select>
                      </div>
                      <div>
                        <label className="text-xs text-slate-400 block mb-1">Monthly Units: {tradeAmount}</label>
                        <input
                          type="range"
                          min="20"
                          max="500"
                          step="10"
                          value={tradeAmount}
                          onChange={(e) => setTradeAmount(Number(e.target.value))}
                          className="w-full"
                        />
                      </div>
                      <div>
                        <button
                          onClick={handleCreateTradeContract}
                          disabled={stance.isAtWar}
                          className="btn btn-primary w-full text-xs flex items-center justify-center gap-2 disabled:opacity-50"
                        >
                          <Send className="w-3.5 h-3.5" /> Sign Treaty
                        </button>
                      </div>
                    </div>
                  </div>

                  {/* Active Contracts Table */}
                  <div className="glass-panel p-4">
                    <h4 className="text-sm font-bold text-slate-200 uppercase tracking-wider mb-2">
                      Active Bilateral Contracts ({activeContracts.length})
                    </h4>
                    {activeContracts.length === 0 ? (
                      <div className="text-xs text-slate-500 italic p-3 text-center">
                        No active commercial treaties established with {targetCountry.name}.
                      </div>
                    ) : (
                      <div className="space-y-2">
                        {activeContracts.map((c: TradeContract) => {
                          const isPlayerImport = c.importerCountryId === player.id;
                          return (
                            <div
                              key={c.id}
                              className="p-3 rounded-lg border border-slate-800 bg-slate-900/60 flex items-center justify-between"
                            >
                              <div>
                                <div className="text-sm font-semibold text-white">
                                  {isPlayerImport ? 'IMPORTING' : 'EXPORTING'} {c.resourceType}
                                </div>
                                <div className="text-xs text-slate-400">
                                  {c.monthlyQuantity} units/mo @ ${(c.monthlyPayment / 1_000_000).toFixed(2)}M/mo
                                </div>
                              </div>
                              <button
                                onClick={() => handleCancelTrade(c.id)}
                                className="p-1.5 text-rose-400 hover:text-rose-300 hover:bg-rose-950/60 rounded-lg transition-colors"
                                title="Terminate Agreement"
                              >
                                <Trash2 className="w-4 h-4" />
                              </button>
                            </div>
                          );
                        })}
                      </div>
                    )}
                  </div>
                </div>
              )}

              {/* TAB 3: COVERT OPERATIONS */}
              {activeTab === 'covert' && (
                <div className="glass-panel p-4 space-y-4">
                  <h4 className="text-sm font-bold text-slate-200 uppercase tracking-wider mb-2 flex items-center gap-2">
                    <Eye className="w-4 h-4 text-purple-400" /> Covert Intelligence & Counter-Operations
                  </h4>
                  <p className="text-xs text-slate-400 mb-3">
                    Deploy intelligence agents to perform espionage, cyber-sabotage, or state fund diversion.
                    Detection risk may severely degrade diplomatic ties.
                  </p>
                  <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
                    {[
                      {
                        type: CovertOperationType.GatherIntelligence,
                        label: 'Gather Intelligence',
                        cost: '$100M',
                        desc: 'Uncovers foreign military unit placements and strategic stockpiles.',
                      },
                      {
                        type: CovertOperationType.SabotageInfrastructure,
                        label: 'Sabotage Production',
                        cost: '$350M',
                        desc: 'Disrupts foreign industrial output and creates economic friction.',
                      },
                      {
                        type: CovertOperationType.EmbezzleFunds,
                        label: 'Cyber Heist',
                        cost: '$500M',
                        desc: 'Infiltrates foreign banking networks to siphon sovereign liquidity.',
                      },
                    ].map((op) => (
                      <button
                        key={op.type}
                        onClick={() => setCovertType(op.type)}
                        className={`p-3 rounded-lg border text-left transition-all ${
                          covertType === op.type
                            ? 'bg-purple-900/30 border-purple-500/70 text-white'
                            : 'bg-slate-900/60 border-slate-800 text-slate-300 hover:bg-slate-800/50'
                        }`}
                      >
                        <div className="text-xs font-bold text-purple-400 mb-1">{op.cost}</div>
                        <div className="text-sm font-semibold mb-1">{op.label}</div>
                        <div className="text-xs text-slate-400">{op.desc}</div>
                      </button>
                    ))}
                  </div>

                  <div className="pt-2 flex justify-end">
                    <button
                      onClick={handleDispatchCovert}
                      disabled={stance.isAtWar}
                      className="btn btn-primary bg-purple-600 hover:bg-purple-500 text-xs flex items-center gap-2"
                    >
                      <Eye className="w-4 h-4" /> Dispatch Operative ({covertType})
                    </button>
                  </div>
                </div>
              )}
            </div>
          ) : (
            <div className="flex-1 flex items-center justify-center p-8 text-slate-500">
              Select a sovereign nation from the left panel to begin bilateral diplomatic proceedings.
            </div>
          )}
        </div>
      </div>
    </div>
  );
};
