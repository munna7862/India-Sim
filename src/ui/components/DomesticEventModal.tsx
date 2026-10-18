import React, { useState } from 'react';
import { GameSession } from '../../domain/models/GameSession';
import { SimulationEngine } from '../../domain/services/SimulationEngine';
import { ResolveEventChoiceCommand } from '../../domain/commands/EventTaskCommands';
import {
  AlertTriangle,
  Flame,
  Check,
  DollarSign,
  TrendingUp,
  Shield,
  Scale,
} from 'lucide-react';

interface DomesticEventModalProps {
  session: GameSession;
  engine: SimulationEngine;
  onUpdate: () => void;
}

export const DomesticEventModal: React.FC<DomesticEventModalProps> = ({
  session,
  engine,
  onUpdate,
}) => {
  const pending = session.pendingEvent;
  const player = session.getPlayerCountry();
  const [errorMsg, setErrorMsg] = useState<string | null>(null);

  if (!pending || !player) return null;

  const handleResolve = (choiceId: string) => {
    const cmd = new ResolveEventChoiceCommand(player.id, pending.eventId, choiceId);
    const res = engine.dispatch(cmd);
    if (res.isSuccess) {
      setErrorMsg(null);
      onUpdate();
    } else {
      setErrorMsg(res.errorMessage || 'Failed to enact executive response.');
    }
  };

  return (
    <div className="panel-overlay z-50 flex items-center justify-center p-4">
      <div
        className="glass-card panel-container max-w-2xl w-full border-2 border-rose-500/70 shadow-2xl shadow-rose-950/80 animate-in fade-in zoom-in-95 duration-200 overflow-hidden"
        onClick={(e) => e.stopPropagation()}
      >
        {/* Urgent Header */}
        <div className="bg-rose-950/80 border-b border-rose-800/80 p-4 flex items-center gap-3">
          <div className="p-2 rounded-lg bg-rose-900/60 text-rose-300 animate-pulse">
            <Flame className="w-6 h-6" />
          </div>
          <div>
            <div className="flex items-center gap-2">
              <span className="text-[10px] font-mono px-2 py-0.5 rounded bg-rose-900 text-rose-200 uppercase font-bold tracking-wider">
                Emergency Flash Directive
              </span>
              <span className="text-xs text-rose-300 font-mono">
                {pending.date?.toString ? pending.date.toString() : 'Current Date'}
              </span>
            </div>
            <h2 className="text-xl font-bold text-white tracking-wide mt-0.5">{pending.title}</h2>
          </div>
        </div>

        {/* Error message */}
        {errorMsg && (
          <div className="mx-5 mt-4 p-3 rounded-lg bg-rose-950 border border-rose-500/80 text-rose-200 text-xs flex items-center gap-2">
            <AlertTriangle className="w-4 h-4 shrink-0" />
            <span>{errorMsg}</span>
          </div>
        )}

        {/* Event Narrative Body */}
        <div className="p-5 space-y-4">
          <div className="p-4 rounded-xl bg-slate-900/80 border border-slate-800 text-slate-200 text-sm leading-relaxed">
            {pending.description}
          </div>

          <div className="text-xs font-mono uppercase text-slate-400 font-bold">
            Executive Decision Trade-Offs (Choose One):
          </div>

          {/* Choices List */}
          <div className="space-y-3">
            {pending.choices.map((choice) => {
              const canAfford = player.treasury >= choice.treasuryCost;

              return (
                <div
                  key={choice.id}
                  className={`p-4 rounded-xl border transition-all ${
                    canAfford
                      ? 'bg-slate-900/70 border-slate-700/80 hover:border-indigo-500/80 hover:bg-slate-800/80'
                      : 'bg-slate-950/40 border-slate-900 opacity-60'
                  }`}
                >
                  <div className="flex flex-col md:flex-row md:items-center justify-between gap-3 mb-2">
                    <h4 className="text-base font-bold text-white">{choice.title}</h4>
                    <button
                      onClick={() => handleResolve(choice.id)}
                      disabled={!canAfford}
                      className="btn btn-primary bg-indigo-600 hover:bg-indigo-500 text-xs shrink-0 flex items-center gap-1.5 py-2 px-4 font-bold disabled:opacity-40"
                    >
                      <Check className="w-4 h-4" /> Ratify Directive
                    </button>
                  </div>

                  <p className="text-xs text-slate-300 mb-3">{choice.description}</p>

                  {/* Deltas & Costs */}
                  <div className="grid grid-cols-2 md:grid-cols-4 gap-2 text-xs pt-2 border-t border-slate-800/80">
                    <div className="flex items-center gap-1.5 text-slate-400">
                      <DollarSign className="w-3.5 h-3.5 text-emerald-400" />
                      Cost: <span className="font-mono font-bold text-white">${(choice.treasuryCost / 1_000_000).toFixed(0)}M</span>
                    </div>

                    <div className="flex items-center gap-1.5 text-slate-400">
                      <TrendingUp className="w-3.5 h-3.5 text-cyan-400" />
                      Approval:{' '}
                      <span className={`font-mono font-bold ${choice.approvalDelta >= 0 ? 'text-emerald-400' : 'text-rose-400'}`}>
                        {choice.approvalDelta >= 0 ? `+${choice.approvalDelta}` : choice.approvalDelta}%
                      </span>
                    </div>

                    <div className="flex items-center gap-1.5 text-slate-400">
                      <Shield className="w-3.5 h-3.5 text-indigo-400" />
                      Stability:{' '}
                      <span className={`font-mono font-bold ${choice.stabilityDelta >= 0 ? 'text-emerald-400' : 'text-rose-400'}`}>
                        {choice.stabilityDelta >= 0 ? `+${choice.stabilityDelta}` : choice.stabilityDelta}%
                      </span>
                    </div>

                    <div className="flex items-center gap-1.5 text-slate-400">
                      <Scale className="w-3.5 h-3.5 text-amber-400" />
                      Corruption:{' '}
                      <span className={`font-mono font-bold ${choice.corruptionDelta <= 0 ? 'text-emerald-400' : 'text-rose-400'}`}>
                        {choice.corruptionDelta >= 0 ? `+${choice.corruptionDelta}` : choice.corruptionDelta}%
                      </span>
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      </div>
    </div>
  );
};
