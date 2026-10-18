import React, { useState } from 'react';
import { GameSession } from '../../domain/models/GameSession';
import { SimulationEngine } from '../../domain/services/SimulationEngine';
import {
  RecruitMilitaryUnitsCommand,
  DisbandMilitaryUnitsCommand,
  ConductArmyDrillsCommand,
  ResolveCombatCommand,
  SignPeaceTreatyCommand,
  CombatReport,
} from '../../domain/commands/DiplomacyMilitaryCommands';
import { LaunchCounterTerrorismCommand } from '../../domain/commands/GovernmentCommands';
import { MilitaryBranch, MilitaryUnitRegistry } from '../../domain/military/MilitaryUnitDefinition';
import {
  ShieldAlert,
  Swords,
  Crosshair,
  Award,
  Shield,
  Plus,
  Minus,
  AlertTriangle,
  X,
  Check,
  Flame,
  Zap,
} from 'lucide-react';

interface MilitaryPanelProps {
  session: GameSession;
  engine: SimulationEngine;
  onClose: () => void;
  onUpdate: () => void;
}

export const MilitaryPanel: React.FC<MilitaryPanelProps> = ({
  session,
  engine,
  onClose,
  onUpdate,
}) => {
  const player = session.getPlayerCountry();
  const [activeTab, setActiveTab] = useState<'units' | 'drills' | 'combat' | 'operations'>('units');
  const [branchFilter, setBranchFilter] = useState<'All' | MilitaryBranch>('All');
  const [feedback, setFeedback] = useState<{ msg: string; isError: boolean } | null>(null);
  const [lastCombatReport, setLastCombatReport] = useState<CombatReport | null>(null);

  if (!player) return null;

  const registry = session.militaryRegistry ?? MilitaryUnitRegistry.createDefaultCatalog();
  const allUnits = registry.getAll();
  const filteredUnits = branchFilter === 'All' ? allUnits : allUnits.filter((u) => u.branch === branchFilter);

  const totalUnits = player.getTotalMilitaryUnits();
  const rawCombatPower = player.calculateTotalCombatPower(registry);
  const rawDefensePower = player.calculateTotalDefensePower(registry);

  const hasDrillsBuff = session.cooldowns.isOnCooldown(player.id, 'ArmyDrills', session.clock.currentDate);
  const drillMultiplier = hasDrillsBuff ? 1.25 : 1.0;
  const effectiveCombatPower = Math.floor(rawCombatPower * drillMultiplier);

  const barracksCount = player.facilities['Barracks'] ?? 0;

  // Active wars
  const activeWars = Object.entries(player.diplomaticRelations)
    .filter(([_, stance]) => stance.isAtWar)
    .map(([targetId]) => session.countries[targetId])
    .filter(Boolean);

  const showMsg = (msg: string, isError: boolean = false) => {
    setFeedback({ msg, isError });
    setTimeout(() => setFeedback(null), 3500);
  };

  const handleRecruit = (unitId: string, quantity: number) => {
    const cmd = new RecruitMilitaryUnitsCommand(player.id, unitId, quantity);
    const res = engine.dispatch(cmd);
    if (res.isSuccess) {
      showMsg(`Commissioned ${quantity}x ${registry.get(unitId).name}`);
      onUpdate();
    } else {
      showMsg(res.errorMessage || 'Failed to commission units', true);
    }
  };

  const handleDisband = (unitId: string, quantity: number) => {
    const cmd = new DisbandMilitaryUnitsCommand(player.id, unitId, quantity);
    const res = engine.dispatch(cmd);
    if (res.isSuccess) {
      showMsg(`Decommissioned ${quantity}x ${unitId}`);
      onUpdate();
    } else {
      showMsg(res.errorMessage || 'Failed to disband units', true);
    }
  };

  const handleConductDrills = () => {
    const cmd = new ConductArmyDrillsCommand(player.id);
    const res = engine.dispatch(cmd);
    if (res.isSuccess) {
      showMsg('Nationwide Joint Armed Forces exercises commenced! (+25% Combat Power for 30 days)');
      onUpdate();
    } else {
      showMsg(res.errorMessage || 'Failed to commence drills', true);
    }
  };

  const handleResolveCombat = (defenderId: string) => {
    const cmd = new ResolveCombatCommand(player.id, defenderId);
    const res = engine.dispatch(cmd);
    if (res.isSuccess) {
      setLastCombatReport(cmd.lastReport);
      showMsg('Tactical theater assault resolved! Review combat action report.');
      onUpdate();
    } else {
      showMsg(res.errorMessage || 'Assault failed to resolve', true);
    }
  };

  const handleSignPeace = (targetId: string) => {
    const cmd = new SignPeaceTreatyCommand(player.id, targetId);
    const res = engine.dispatch(cmd);
    if (res.isSuccess) {
      showMsg(`Peace Accord ratified with ${session.countries[targetId]?.name}`);
      onUpdate();
    } else {
      showMsg(res.errorMessage || 'Peace Accord negotiation failed', true);
    }
  };

  const handleCounterTerrorism = () => {
    const cmd = new LaunchCounterTerrorismCommand(player.id);
    const res = engine.dispatch(cmd);
    if (res.isSuccess) {
      showMsg('Counter-Terrorism task force strike successful! Domestic stability restored.');
      onUpdate();
    } else {
      showMsg(res.errorMessage || 'Counter-Terrorism mobilization failed', true);
    }
  };

  return (
    <div className="panel-overlay" onClick={onClose}>
      <div
        className="glass-card panel-container max-w-5xl w-full max-h-[90vh] flex flex-col overflow-hidden"
        onClick={(e) => e.stopPropagation()}
      >
        {/* Header */}
        <div className="p-4 border-b border-slate-700/60 flex items-center justify-between bg-slate-900/60">
          <div className="flex items-center gap-3">
            <ShieldAlert className="w-6 h-6 text-rose-500" />
            <div>
              <h2 className="text-xl font-bold text-white tracking-wide">Ministry of Defence & Joint Staff</h2>
              <p className="text-xs text-slate-400">Order of Battle, Unit Procurement, Readiness Drills & Frontline Operations</p>
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

        {/* Top Readiness & Stats Bar */}
        <div className="p-4 border-b border-slate-800/80 bg-slate-950/40 grid grid-cols-2 md:grid-cols-4 gap-3">
          <div className="glass-panel p-2.5">
            <div className="text-xs text-slate-400">Total Combat Units</div>
            <div className="text-xl font-bold text-white font-mono">{totalUnits.toLocaleString()}</div>
            <div className="text-[10px] text-slate-500">Active Duty Formations</div>
          </div>
          <div className="glass-panel p-2.5">
            <div className="text-xs text-slate-400">Combat Power (Offense)</div>
            <div className="text-xl font-bold text-rose-400 font-mono flex items-center gap-1.5">
              {effectiveCombatPower.toLocaleString()}
              {hasDrillsBuff && <span className="text-[10px] px-1.5 py-0.5 rounded bg-emerald-950 border border-emerald-500/50 text-emerald-300 font-sans">+25% Drill</span>}
            </div>
            <div className="text-[10px] text-slate-500">Base: {rawCombatPower.toLocaleString()}</div>
          </div>
          <div className="glass-panel p-2.5">
            <div className="text-xs text-slate-400">Homeland Defense Power</div>
            <div className="text-xl font-bold text-cyan-400 font-mono">{rawDefensePower.toLocaleString()}</div>
            <div className="text-[10px] text-slate-500">Fortification & Interception</div>
          </div>
          <div className="glass-panel p-2.5">
            <div className="text-xs text-slate-400">Operational Barracks</div>
            <div className="text-xl font-bold text-amber-400 font-mono">{barracksCount} Facilities</div>
            <div className="text-[10px] text-slate-500">Limits Advanced Unit Types</div>
          </div>
        </div>

        {/* Navigation Tabs */}
        <div className="px-4 pt-3 flex items-center justify-between border-b border-slate-800/80 bg-slate-900/30">
          <div className="flex items-center gap-2">
            <button
              onClick={() => setActiveTab('units')}
              className={`px-3 py-2 text-sm font-semibold rounded-t-lg transition-colors flex items-center gap-2 border-b-2 ${
                activeTab === 'units'
                  ? 'border-rose-500 text-rose-400 bg-rose-500/10'
                  : 'border-transparent text-slate-400 hover:text-white'
              }`}
            >
              <Swords className="w-4 h-4" /> Unit Roster & Commissioning
            </button>
            <button
              onClick={() => setActiveTab('drills')}
              className={`px-3 py-2 text-sm font-semibold rounded-t-lg transition-colors flex items-center gap-2 border-b-2 ${
                activeTab === 'drills'
                  ? 'border-rose-500 text-rose-400 bg-rose-500/10'
                  : 'border-transparent text-slate-400 hover:text-white'
              }`}
            >
              <Award className="w-4 h-4" /> Readiness Drills
              {hasDrillsBuff && <span className="w-2 h-2 rounded-full bg-emerald-400 animate-pulse" />}
            </button>
            <button
              onClick={() => setActiveTab('combat')}
              className={`px-3 py-2 text-sm font-semibold rounded-t-lg transition-colors flex items-center gap-2 border-b-2 ${
                activeTab === 'combat'
                  ? 'border-rose-500 text-rose-400 bg-rose-500/10'
                  : 'border-transparent text-slate-400 hover:text-white'
              }`}
            >
              <Crosshair className="w-4 h-4" /> War Theaters ({activeWars.length})
              {activeWars.length > 0 && <span className="w-2 h-2 rounded-full bg-rose-500 animate-ping" />}
            </button>
            <button
              onClick={() => setActiveTab('operations')}
              className={`px-3 py-2 text-sm font-semibold rounded-t-lg transition-colors flex items-center gap-2 border-b-2 ${
                activeTab === 'operations'
                  ? 'border-rose-500 text-rose-400 bg-rose-500/10'
                  : 'border-transparent text-slate-400 hover:text-white'
              }`}
            >
              <Shield className="w-4 h-4" /> Special Operations
            </button>
          </div>
        </div>

        {/* Content Area */}
        <div className="flex-1 overflow-y-auto p-4">
          {/* TAB 1: UNITS */}
          {activeTab === 'units' && (
            <div className="space-y-4">
              {/* Branch Filter Pills */}
              <div className="flex items-center gap-2">
                <span className="text-xs text-slate-400 mr-2">Military Branch:</span>
                {(['All', MilitaryBranch.Land, MilitaryBranch.Air, MilitaryBranch.Naval] as const).map((b) => (
                  <button
                    key={b}
                    onClick={() => setBranchFilter(b)}
                    className={`px-2.5 py-1 rounded-lg text-xs font-semibold transition-colors ${
                      branchFilter === b
                        ? 'bg-rose-600 text-white shadow-sm shadow-rose-500/30'
                        : 'bg-slate-800 text-slate-400 hover:bg-slate-700 hover:text-white'
                    }`}
                  >
                    {b}
                  </button>
                ))}
              </div>

              {/* Units Grid */}
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-3">
                {filteredUnits.map((u) => {
                  const currentDeployed = player.militaryUnits[u.id] ?? 0;
                  const canAfford1 =
                    player.treasury >= u.costTreasury &&
                    player.stockpiles.iron >= u.costIron &&
                    player.stockpiles.oil >= u.costOil &&
                    player.stockpiles.uranium >= u.costUranium &&
                    barracksCount >= u.requiredBarracks;

                  return (
                    <div
                      key={u.id}
                      className="glass-panel p-3 flex flex-col justify-between border border-slate-800/80 hover:border-slate-700 transition-all"
                    >
                      <div>
                        <div className="flex items-center justify-between mb-1">
                          <span className="text-xs font-mono px-1.5 py-0.5 rounded bg-slate-800 text-slate-300 font-bold">
                            {u.branch}
                          </span>
                          <span className="text-xs font-mono font-bold text-rose-400">
                            {currentDeployed} In Service
                          </span>
                        </div>
                        <h4 className="text-sm font-bold text-white mb-1">{u.name}</h4>

                        {/* Power & Upkeep */}
                        <div className="grid grid-cols-2 gap-1 text-[11px] text-slate-400 mb-2">
                          <div>Combat: <span className="font-bold text-slate-200">+{u.combatPower}</span></div>
                          <div>Defense: <span className="font-bold text-slate-200">+{u.defensePower}</span></div>
                          <div className="col-span-2">
                            Upkeep: <span className="font-bold text-slate-300">${(u.monthlyUpkeepTreasury / 1_000_000).toFixed(1)}M/mo</span>
                          </div>
                        </div>

                        {/* Cost & Requirements */}
                        <div className="text-[10px] text-slate-400 bg-slate-900/60 p-2 rounded border border-slate-800/60 space-y-0.5 mb-3">
                          <div>Cost: ${(u.costTreasury / 1_000_000).toFixed(0)}M Treasury</div>
                          <div>
                            Resources: {u.costIron > 0 ? `${u.costIron} Iron ` : ''}
                            {u.costOil > 0 ? `${u.costOil} Oil ` : ''}
                            {u.costUranium > 0 ? `${u.costUranium} Uranium` : ''}
                            {u.costIron === 0 && u.costOil === 0 && u.costUranium === 0 ? 'None' : ''}
                          </div>
                          <div>
                            Requires: {u.requiredBarracks} Barracks (You have {barracksCount})
                          </div>
                        </div>
                      </div>

                      {/* Action Buttons */}
                      <div className="flex items-center gap-1.5 pt-2 border-t border-slate-800/80">
                        <button
                          onClick={() => handleRecruit(u.id, 1)}
                          disabled={!canAfford1}
                          className="btn btn-primary bg-rose-600 hover:bg-rose-500 text-xs flex-1 flex items-center justify-center gap-1 py-1.5 disabled:opacity-40"
                        >
                          <Plus className="w-3 h-3" /> +1
                        </button>
                        <button
                          onClick={() => handleRecruit(u.id, 5)}
                          disabled={!canAfford1 || player.treasury < u.costTreasury * 5}
                          className="btn btn-primary bg-rose-700 hover:bg-rose-600 text-xs flex-1 flex items-center justify-center gap-1 py-1.5 disabled:opacity-40"
                        >
                          <Plus className="w-3 h-3" /> +5
                        </button>
                        <button
                          onClick={() => handleDisband(u.id, 1)}
                          disabled={currentDeployed <= 0}
                          className="btn btn-neutral text-xs px-2 py-1.5 text-slate-400 hover:text-rose-300 disabled:opacity-30"
                          title="Decommission unit"
                        >
                          <Minus className="w-3 h-3" />
                        </button>
                      </div>
                    </div>
                  );
                })}
              </div>
            </div>
          )}

          {/* TAB 2: DRILLS */}
          {activeTab === 'drills' && (
            <div className="glass-panel p-5 max-w-2xl mx-auto space-y-4">
              <div className="flex items-center gap-3">
                <Award className="w-8 h-8 text-amber-400" />
                <div>
                  <h3 className="text-lg font-bold text-white">Nationwide Military Exercises & Drills</h3>
                  <p className="text-xs text-slate-400">Joint staff command war games and tactical live-fire field maneuvers.</p>
                </div>
              </div>

              <div className="p-4 rounded-lg bg-slate-900/60 border border-slate-800 space-y-2 text-xs text-slate-300">
                <div className="flex justify-between">
                  <span className="text-slate-400">Exercise Budget:</span>
                  <span className="font-bold text-white">$100 Million</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-slate-400">Operational Duration:</span>
                  <span className="font-bold text-white">30 Days</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-slate-400">Tactical Readiness Bonus:</span>
                  <span className="font-bold text-emerald-400">+25% Combat Power Multiplier</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-slate-400">Readiness Cooldown:</span>
                  <span className="font-bold text-slate-400">180 Days</span>
                </div>
              </div>

              {hasDrillsBuff ? (
                <div className="p-3 rounded-lg bg-emerald-950/60 border border-emerald-500/50 text-emerald-200 text-sm flex items-center gap-2">
                  <Check className="w-4 h-4 text-emerald-400 shrink-0" />
                  <span>Forces are operating under drill readiness bonus (+25% power active).</span>
                </div>
              ) : (
                <button
                  onClick={handleConductDrills}
                  disabled={player.treasury < 100_000_000}
                  className="btn btn-primary bg-amber-600 hover:bg-amber-500 w-full flex items-center justify-center gap-2 py-2.5 text-sm font-bold"
                >
                  <Flame className="w-4 h-4" /> Commence Army Drills ($100M)
                </button>
              )}
            </div>
          )}

          {/* TAB 3: COMBAT THEATER */}
          {activeTab === 'combat' && (
            <div className="space-y-4">
              {activeWars.length === 0 ? (
                <div className="glass-panel p-8 text-center max-w-lg mx-auto">
                  <Shield className="w-10 h-10 text-emerald-400 mx-auto mb-2" />
                  <h3 className="text-base font-bold text-white">Homeland Peace Intact</h3>
                  <p className="text-xs text-slate-400 mt-1">
                    No active hostilities or frontline wars declared. You can declare war via the Ministry of External Affairs if necessary.
                  </p>
                </div>
              ) : (
                <div className="space-y-4">
                  {activeWars.map((foe) => {
                    const foeUnits = foe.getTotalMilitaryUnits();
                    const foeDefPower = foe.calculateTotalDefensePower(registry);

                    return (
                      <div
                        key={foe.id}
                        className="glass-panel p-4 border border-rose-900/60 bg-rose-950/10 space-y-3"
                      >
                        <div className="flex flex-col md:flex-row md:items-center justify-between gap-2 border-b border-rose-900/40 pb-2">
                          <div className="flex items-center gap-2">
                            <Swords className="w-5 h-5 text-rose-500" />
                            <div>
                              <h4 className="text-base font-bold text-white">War Front: {foe.name}</h4>
                              <p className="text-xs text-rose-300">Active High-Intensity Combat Theater</p>
                            </div>
                          </div>
                          <div className="flex items-center gap-2">
                            <button
                              onClick={() => handleResolveCombat(foe.id)}
                              className="btn btn-primary bg-rose-600 hover:bg-rose-500 text-xs flex items-center gap-1.5"
                            >
                              <Crosshair className="w-4 h-4" /> Launch Tactical Offensive
                            </button>
                            <button
                              onClick={() => handleSignPeace(foe.id)}
                              className="btn btn-neutral text-xs text-slate-300 hover:text-white"
                            >
                              Negotiate Ceasefire
                            </button>
                          </div>
                        </div>

                        {/* Force Balance Comparison */}
                        <div className="grid grid-cols-2 gap-4 text-xs">
                          <div className="p-2.5 rounded bg-slate-900/60 border border-slate-800">
                            <div className="text-slate-400 mb-1">Friendly Forces ({player.name})</div>
                            <div className="font-mono text-sm font-bold text-white">{totalUnits.toLocaleString()} units</div>
                            <div className="font-mono text-xs text-rose-400">{effectiveCombatPower.toLocaleString()} Offensive Power</div>
                          </div>
                          <div className="p-2.5 rounded bg-slate-900/60 border border-slate-800">
                            <div className="text-slate-400 mb-1">Enemy Forces ({foe.name})</div>
                            <div className="font-mono text-sm font-bold text-white">{foeUnits.toLocaleString()} units</div>
                            <div className="font-mono text-xs text-cyan-400">{foeDefPower.toLocaleString()} Defense Fortification</div>
                          </div>
                        </div>
                      </div>
                    );
                  })}
                </div>
              )}

              {/* Last Combat Report Card */}
              {lastCombatReport && (
                <div className="glass-panel p-4 border border-slate-700 bg-slate-900/80 space-y-2">
                  <div className="text-xs font-mono uppercase text-slate-400">After-Action Combat Report</div>
                  <div className="text-base font-bold text-white">{lastCombatReport.narrative}</div>
                  <div className="grid grid-cols-2 md:grid-cols-4 gap-2 text-xs pt-2">
                    <div>Outcome: <span className="font-bold text-amber-400">{lastCombatReport.outcome}</span></div>
                    <div>Attacker Casualties: <span className="font-bold text-rose-400">{lastCombatReport.attackerCasualties}</span></div>
                    <div>Defender Casualties: <span className="font-bold text-emerald-400">{lastCombatReport.defenderCasualties}</span></div>
                    <div>Date: <span className="font-bold text-slate-300">{lastCombatReport.date.toString()}</span></div>
                  </div>
                </div>
              )}
            </div>
          )}

          {/* TAB 4: SPECIAL OPERATIONS */}
          {activeTab === 'operations' && (
            <div className="glass-panel p-5 max-w-2xl mx-auto space-y-4">
              <div className="flex items-center gap-3">
                <Shield className="w-8 h-8 text-cyan-400" />
                <div>
                  <h3 className="text-lg font-bold text-white">Special Internal Security Operations</h3>
                  <p className="text-xs text-slate-400">Homeland security, anti-terror commandos, and stabilization raids.</p>
                </div>
              </div>

              <div className="p-4 rounded-lg bg-slate-900/60 border border-slate-800 space-y-3">
                <div className="flex items-center justify-between">
                  <div>
                    <h4 className="text-sm font-bold text-white">Counter-Terrorism Task Force Strike</h4>
                    <p className="text-xs text-slate-400">
                      Mobilizes elite tactical units to neutralize insurgent threats. Grants +4% Stability, +3% Approval, and -3% Corruption (Cost: $200M, 60-day cooldown).
                    </p>
                  </div>
                  <button
                    onClick={handleCounterTerrorism}
                    disabled={player.treasury < 200_000_000}
                    className="btn btn-primary bg-cyan-600 hover:bg-cyan-500 text-xs shrink-0 flex items-center gap-1.5"
                  >
                    <Zap className="w-3.5 h-3.5" /> Mobilize Strike ($200M)
                  </button>
                </div>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};
