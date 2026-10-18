import React, { useState } from 'react';
import { GameSession } from '../../domain/models/GameSession';
import { SimulationEngine } from '../../domain/services/SimulationEngine';
import { ClaimAchievementRewardCommand } from '../../domain/commands/EventTaskCommands';
import { AchievementRegistry } from '../../domain/achievements/AchievementRegistry';
import { AchievementDefinition } from '../../domain/achievements/Achievement';
import {
  Trophy,
  Sparkles,
  CheckCircle2,
  Lock,
  X,
  AlertTriangle,
  Check,
  Star,
} from 'lucide-react';

interface AchievementsPanelProps {
  session: GameSession;
  engine: SimulationEngine;
  onClose: () => void;
  onUpdate: () => void;
}

export const AchievementsPanel: React.FC<AchievementsPanelProps> = ({
  session,
  engine,
  onClose,
  onUpdate,
}) => {
  const player = session.getPlayerCountry();
  const [feedback, setFeedback] = useState<{ msg: string; isError: boolean } | null>(null);

  if (!player) return null;

  const registry = session.achievementRegistry ?? AchievementRegistry.createDefaultCatalog();
  const allAchievements = registry.getAll();

  const unlockedCount = Object.values(session.achievements).filter((a) => a.isUnlocked).length;
  const totalPrestige = Object.entries(session.achievements).reduce((sum, [id, prog]) => {
    if (prog.isUnlocked) {
      const def = registry.tryGet(id);
      return sum + (def?.rewardPrestigeScore ?? 0);
    }
    return sum;
  }, 0);

  const showMsg = (msg: string, isError: boolean = false) => {
    setFeedback({ msg, isError });
    setTimeout(() => setFeedback(null), 3500);
  };

  const handleClaimReward = (id: string) => {
    const cmd = new ClaimAchievementRewardCommand(id, player.id);
    const res = engine.dispatch(cmd);
    if (res.isSuccess) {
      showMsg('National achievement reward and prestige claimed!');
      onUpdate();
    } else {
      showMsg(res.errorMessage || 'Failed to claim achievement reward', true);
    }
  };

  return (
    <div className="panel-overlay" onClick={onClose}>
      <div
        className="glass-card panel-container max-w-4xl w-full max-h-[90vh] flex flex-col overflow-hidden"
        onClick={(e) => e.stopPropagation()}
      >
        {/* Header */}
        <div className="p-4 border-b border-slate-700/60 flex items-center justify-between bg-slate-900/60">
          <div className="flex items-center gap-3">
            <Trophy className="w-6 h-6 text-amber-400" />
            <div>
              <h2 className="text-xl font-bold text-white tracking-wide">National Hall of Achievements</h2>
              <p className="text-xs text-slate-400">
                Sovereign Milestones, Global Dominance & Historical Legacies
              </p>
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

        {/* Top Summary Bar */}
        <div className="p-4 border-b border-slate-800/80 bg-slate-950/40 grid grid-cols-2 md:grid-cols-3 gap-3">
          <div className="glass-panel p-2.5">
            <div className="text-xs text-slate-400">Unlocked Milestones</div>
            <div className="text-xl font-bold text-amber-400 font-mono">
              {unlockedCount} / {allAchievements.length}
            </div>
          </div>
          <div className="glass-panel p-2.5">
            <div className="text-xs text-slate-400">Total Prestige Points</div>
            <div className="text-xl font-bold text-purple-400 font-mono flex items-center gap-1.5">
              <Star className="w-4 h-4 fill-purple-400" /> {totalPrestige}
            </div>
          </div>
          <div className="glass-panel p-2.5 col-span-2 md:col-span-1">
            <div className="text-xs text-slate-400">Status</div>
            <div className="text-sm font-semibold text-emerald-300">
              {unlockedCount === allAchievements.length ? 'National Hegemon' : 'Ascending Sovereign'}
            </div>
          </div>
        </div>

        {/* Achievements Grid */}
        <div className="flex-1 overflow-y-auto p-4 grid grid-cols-1 md:grid-cols-2 gap-3">
          {allAchievements.map((def: AchievementDefinition) => {
            const prog = session.achievements[def.id];
            const isUnlocked = prog?.isUnlocked ?? false;
            const isClaimed = prog?.isRewardClaimed || session.claimedAchievementRewardIds.includes(def.id);
            const pct = Math.floor(prog?.progressPercent ?? 0);

            return (
              <div
                key={def.id}
                className={`glass-panel p-4 border transition-all flex flex-col justify-between ${
                  isUnlocked
                    ? 'border-amber-500/40 bg-amber-950/10'
                    : 'border-slate-800/80 bg-slate-900/30'
                }`}
              >
                <div>
                  <div className="flex items-center justify-between mb-1.5">
                    <span className="text-[10px] px-2 py-0.5 rounded bg-slate-800 font-mono text-slate-300 font-bold uppercase">
                      {def.category}
                    </span>
                    {isUnlocked ? (
                      <span className="badge badge-success text-[10px] flex items-center gap-1">
                        <CheckCircle2 className="w-3 h-3 text-emerald-400" /> Unlocked
                      </span>
                    ) : (
                      <span className="badge badge-neutral text-[10px] flex items-center gap-1">
                        <Lock className="w-3 h-3 text-slate-500" /> Locked
                      </span>
                    )}
                  </div>

                  <h4 className="text-base font-bold text-white mb-1 flex items-center gap-1.5">
                    <Trophy className={`w-4 h-4 ${isUnlocked ? 'text-amber-400' : 'text-slate-600'}`} />
                    {def.title}
                  </h4>
                  <p className="text-xs text-slate-400 mb-3">{def.description}</p>
                </div>

                <div>
                  {/* Progress bar */}
                  <div className="mb-3">
                    <div className="flex justify-between text-[11px] text-slate-400 mb-1">
                      <span>Progress</span>
                      <span className="font-mono">{pct}%</span>
                    </div>
                    <div className="w-full h-1.5 bg-slate-800 rounded-full overflow-hidden">
                      <div
                        className={`h-full rounded-full transition-all duration-300 ${
                          isUnlocked ? 'bg-amber-400' : 'bg-indigo-500'
                        }`}
                        style={{ width: `${Math.min(100, Math.max(0, pct))}%` }}
                      />
                    </div>
                  </div>

                  {/* Rewards Footer */}
                  <div className="pt-2 border-t border-slate-800 flex items-center justify-between">
                    <div className="text-[11px] text-slate-400">
                      Reward:{' '}
                      <span className="text-emerald-400 font-bold">
                        +${(def.rewardTreasury / 1_000_000).toFixed(0)}M
                      </span>{' '}
                      |{' '}
                      <span className="text-purple-400 font-bold">
                        +{def.rewardPrestigeScore} Prestige
                      </span>
                    </div>

                    {isUnlocked && !isClaimed && (
                      <button
                        onClick={() => handleClaimReward(def.id)}
                        className="btn btn-primary bg-amber-600 hover:bg-amber-500 text-[11px] py-1 px-2.5 font-bold flex items-center gap-1 animate-pulse shadow-sm shadow-amber-500/30"
                      >
                        <Sparkles className="w-3 h-3" /> Claim
                      </button>
                    )}
                    {isClaimed && (
                      <span className="text-[10px] text-emerald-400 font-semibold flex items-center gap-0.5">
                        <Check className="w-3 h-3" /> Claimed
                      </span>
                    )}
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      </div>
    </div>
  );
};
