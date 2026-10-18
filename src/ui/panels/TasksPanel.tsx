import React, { useState } from 'react';
import { GameSession } from '../../domain/models/GameSession';
import { SimulationEngine } from '../../domain/services/SimulationEngine';
import { ClaimTaskRewardCommand } from '../../domain/commands/EventTaskCommands';
import { GameTaskStatus } from '../../domain/models/types';
import { TaskCategory } from '../../domain/tasks/GameTask';
import {
  CheckSquare,
  Sparkles,
  TrendingUp,
  HeartHandshake,
  ShieldAlert,
  Globe2,
  CheckCircle2,
  Circle,
  AlertTriangle,
  X,
  Check,
  Award,
} from 'lucide-react';

interface TasksPanelProps {
  session: GameSession;
  engine: SimulationEngine;
  onClose: () => void;
  onUpdate: () => void;
}

export const TasksPanel: React.FC<TasksPanelProps> = ({
  session,
  engine,
  onClose,
  onUpdate,
}) => {
  const player = session.getPlayerCountry();
  const [selectedCategory, setSelectedCategory] = useState<'All' | TaskCategory>('All');
  const [feedback, setFeedback] = useState<{ msg: string; isError: boolean } | null>(null);

  if (!player) return null;

  const allTasks = Object.values(session.tasks);
  const filteredTasks =
    selectedCategory === 'All'
      ? allTasks
      : allTasks.filter((t) => t.category === selectedCategory);

  const completedCount = allTasks.filter(
    (t) => t.status === GameTaskStatus.Completed
  ).length;

  const showMsg = (msg: string, isError: boolean = false) => {
    setFeedback({ msg, isError });
    setTimeout(() => setFeedback(null), 3500);
  };

  const handleClaimReward = (taskId: string) => {
    const cmd = new ClaimTaskRewardCommand(player.id, taskId);
    const res = engine.dispatch(cmd);
    if (res.isSuccess) {
      showMsg('Task reward claimed successfully!');
      onUpdate();
    } else {
      showMsg(res.errorMessage || 'Failed to claim reward', true);
    }
  };

  const getCategoryIcon = (cat: TaskCategory) => {
    switch (cat) {
      case TaskCategory.Economic:
        return <TrendingUp className="w-3.5 h-3.5 text-emerald-400" />;
      case TaskCategory.Welfare:
        return <HeartHandshake className="w-3.5 h-3.5 text-cyan-400" />;
      case TaskCategory.Defense:
        return <ShieldAlert className="w-3.5 h-3.5 text-rose-400" />;
      case TaskCategory.Diplomatic:
        return <Globe2 className="w-3.5 h-3.5 text-indigo-400" />;
      default:
        return <CheckSquare className="w-3.5 h-3.5 text-slate-400" />;
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
            <CheckSquare className="w-6 h-6 text-emerald-400" />
            <div>
              <h2 className="text-xl font-bold text-white tracking-wide">Presidential Directives & Tasks</h2>
              <p className="text-xs text-slate-400">
                National Strategic Goals, Modernization Mandates & Executive Objectives
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
        <div className="p-4 border-b border-slate-800/80 bg-slate-950/40 flex items-center justify-between">
          <div className="flex items-center gap-2">
            <Award className="w-5 h-5 text-amber-400" />
            <span className="text-sm text-slate-300">
              Completed:{' '}
              <span className="font-bold text-white font-mono">
                {completedCount} / {allTasks.length} Directives
              </span>
            </span>
          </div>

          {/* Category Filter Pills */}
          <div className="flex items-center gap-1.5">
            {(['All', TaskCategory.Economic, TaskCategory.Welfare, TaskCategory.Defense, TaskCategory.Diplomatic] as const).map(
              (cat) => (
                <button
                  key={cat}
                  onClick={() => setSelectedCategory(cat)}
                  className={`px-2.5 py-1 rounded-lg text-xs font-semibold transition-colors flex items-center gap-1.5 ${
                    selectedCategory === cat
                      ? 'bg-emerald-600 text-white shadow-sm shadow-emerald-500/30'
                      : 'bg-slate-800 text-slate-400 hover:bg-slate-700 hover:text-white'
                  }`}
                >
                  {cat !== 'All' && getCategoryIcon(cat as TaskCategory)}
                  {cat}
                </button>
              )
            )}
          </div>
        </div>

        {/* Tasks List */}
        <div className="flex-1 overflow-y-auto p-4 space-y-3">
          {filteredTasks.length === 0 ? (
            <div className="glass-panel p-8 text-center text-slate-500 text-sm">
              No tasks found under this category filter.
            </div>
          ) : (
            filteredTasks.map((t) => {
              const isClaimed = t.isRewardClaimed || session.claimedRewardIds.includes(t.id);
              const isCompleted =
                typeof (t as any).checkCompletion === 'function'
                  ? (t as any).checkCompletion()
                  : t.status === GameTaskStatus.Completed ||
                    (t.subTasks &&
                      t.subTasks.length > 0 &&
                      t.subTasks.every((st) => st.currentCount >= st.targetCount));

              const pct = typeof (t as any).progressPercentage === 'number'
                ? Math.floor((t as any).progressPercentage)
                : isCompleted ? 100 : 0;

              return (
                <div
                  key={t.id}
                  className={`glass-panel p-4 border transition-all ${
                    isCompleted
                      ? 'border-emerald-500/40 bg-emerald-950/10'
                      : 'border-slate-800/80 bg-slate-900/40 hover:border-slate-700'
                  }`}
                >
                  <div className="flex flex-col md:flex-row md:items-center justify-between gap-3 mb-2">
                    <div className="flex items-start gap-2.5">
                      <div className="mt-0.5">{getCategoryIcon(t.category)}</div>
                      <div>
                        <div className="flex items-center gap-2">
                          <h4 className="text-base font-bold text-white">{t.title}</h4>
                          {t.isPrincipal && (
                            <span className="text-[10px] px-1.5 py-0.5 rounded bg-amber-950 border border-amber-500/50 text-amber-300 font-bold uppercase">
                              Principal Mandate
                            </span>
                          )}
                          <span className="text-[10px] px-1.5 py-0.5 rounded bg-slate-800 text-slate-400 font-mono">
                            {t.category}
                          </span>
                        </div>
                        <p className="text-xs text-slate-400 mt-0.5">{t.description}</p>
                      </div>
                    </div>

                    {/* Claim Button / Badge */}
                    <div className="shrink-0">
                      {isClaimed ? (
                        <span className="badge badge-neutral text-xs text-slate-400 flex items-center gap-1">
                          <Check className="w-3.5 h-3.5 text-emerald-400" /> Reward Claimed
                        </span>
                      ) : isCompleted ? (
                        <button
                          onClick={() => handleClaimReward(t.id)}
                          className="btn btn-primary bg-emerald-600 hover:bg-emerald-500 text-xs font-bold flex items-center gap-1.5 animate-pulse shadow-md shadow-emerald-500/30"
                        >
                          <Sparkles className="w-4 h-4 text-amber-300" /> Claim Reward
                        </button>
                      ) : (
                        <span className="badge badge-info text-xs">
                          {pct}% In Progress
                        </span>
                      )}
                    </div>
                  </div>

                  {/* Sub-tasks Breakdown */}
                  {t.subTasks && t.subTasks.length > 0 && (
                    <div className="space-y-1.5 mt-3 pt-3 border-t border-slate-800/80">
                      {t.subTasks.map((st, i) => {
                        const stDone = st.currentCount >= st.targetCount;
                        return (
                          <div
                            key={i}
                            className="flex items-center justify-between text-xs text-slate-300"
                          >
                            <div className="flex items-center gap-2">
                              {stDone ? (
                                <CheckCircle2 className="w-3.5 h-3.5 text-emerald-400 shrink-0" />
                              ) : (
                                <Circle className="w-3.5 h-3.5 text-slate-500 shrink-0" />
                              )}
                              <span>{st.description}</span>
                            </div>
                            <span className="font-mono text-slate-400">
                              {st.currentCount} / {st.targetCount}
                            </span>
                          </div>
                        );
                      })}
                    </div>
                  )}

                  {/* Progress Bar & Reward Footer */}
                  <div className="mt-3 pt-2.5 border-t border-slate-800/80 flex flex-col md:flex-row md:items-center justify-between gap-2 text-xs">
                    {/* Progress Bar */}
                    <div className="flex items-center gap-2 flex-1 max-w-xs">
                      <div className="w-full h-1.5 bg-slate-800 rounded-full overflow-hidden">
                        <div
                          className="h-full bg-emerald-500 rounded-full transition-all duration-300"
                          style={{ width: `${Math.min(100, Math.max(0, pct))}%` }}
                        />
                      </div>
                      <span className="font-mono text-[11px] text-slate-400 shrink-0">{pct}%</span>
                    </div>

                    {/* Reward preview */}
                    <div className="text-[11px] text-slate-400 flex items-center gap-2">
                      <span>Reward:</span>
                      {t.reward.treasuryBonus > 0 && (
                        <span className="text-emerald-400 font-bold">
                          +${(t.reward.treasuryBonus / 1_000_000).toFixed(0)}M Treasury
                        </span>
                      )}
                      {t.reward.approvalBonus > 0 && (
                        <span className="text-cyan-400 font-bold">
                          +{t.reward.approvalBonus}% Approval
                        </span>
                      )}
                      {t.reward.stabilityBonus > 0 && (
                        <span className="text-indigo-400 font-bold">
                          +{t.reward.stabilityBonus}% Stability
                        </span>
                      )}
                    </div>
                  </div>
                </div>
              );
            })
          )}
        </div>
      </div>
    </div>
  );
};
