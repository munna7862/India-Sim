import React, { useState } from 'react';
import { GameSession } from '../../domain/models/GameSession';
import { SimulationEngine } from '../../domain/services/SimulationEngine';
import { DismissNotificationCommand } from '../../domain/commands/EventTaskCommands';
import { EventCategory, EventSeverity } from '../../domain/models/types';
import {
  Bell,
  X,
  TrendingUp,
  Landmark,
  Globe2,
  ShieldAlert,
  Flame,
  CheckCheck,
  AlertTriangle,
  Info,
} from 'lucide-react';

interface EventLogDrawerProps {
  session: GameSession;
  engine: SimulationEngine;
  isOpen: boolean;
  onClose: () => void;
  onUpdate: () => void;
}

export const EventLogDrawer: React.FC<EventLogDrawerProps> = ({
  session,
  engine,
  isOpen,
  onClose,
  onUpdate,
}) => {
  const [selectedCategory, setSelectedCategory] = useState<'All' | EventCategory>('All');

  if (!isOpen) return null;

  const events = session.events || [];
  const filteredEvents =
    selectedCategory === 'All'
      ? events
      : events.filter((e) => e.category === selectedCategory);

  const reversedEvents = [...filteredEvents].reverse();

  const handleDismissAll = () => {
    const cmd = new DismissNotificationCommand();
    const res = engine.dispatch(cmd);
    if (res.isSuccess) {
      onUpdate();
    }
  };

  const getCategoryIcon = (cat: EventCategory) => {
    switch (cat) {
      case EventCategory.Economy:
        return <TrendingUp className="w-4 h-4 text-emerald-400" />;
      case EventCategory.Government:
        return <Landmark className="w-4 h-4 text-cyan-400" />;
      case EventCategory.Diplomacy:
        return <Globe2 className="w-4 h-4 text-indigo-400" />;
      case EventCategory.Military:
        return <ShieldAlert className="w-4 h-4 text-rose-400" />;
      case EventCategory.Emergency:
        return <Flame className="w-4 h-4 text-amber-400" />;
      default:
        return <Bell className="w-4 h-4 text-slate-400" />;
    }
  };

  const getSeverityBadge = (sev: EventSeverity) => {
    switch (sev) {
      case EventSeverity.Emergency:
        return (
          <span className="badge badge-danger text-[10px] flex items-center gap-1">
            <Flame className="w-2.5 h-2.5" /> Emergency
          </span>
        );
      case EventSeverity.Warning:
        return (
          <span className="badge badge-warning text-[10px] flex items-center gap-1">
            <AlertTriangle className="w-2.5 h-2.5" /> Warning
          </span>
        );
      case EventSeverity.Info:
      default:
        return (
          <span className="badge badge-neutral text-[10px] flex items-center gap-1">
            <Info className="w-2.5 h-2.5" /> Info
          </span>
        );
    }
  };

  return (
    <div className="panel-overlay z-50 flex justify-end" onClick={onClose}>
      <div
        className="glass-card w-full max-w-md h-full flex flex-col border-l border-slate-700/80 shadow-2xl animate-in slide-in-from-right duration-200"
        onClick={(e) => e.stopPropagation()}
      >
        {/* Header */}
        <div className="p-4 border-b border-slate-800 bg-slate-900/80 flex items-center justify-between">
          <div className="flex items-center gap-2.5">
            <Bell className="w-5 h-5 text-indigo-400" />
            <h3 className="text-base font-bold text-white tracking-wide">National Intelligence Wire</h3>
            <span className="text-xs px-2 py-0.5 rounded-full bg-slate-800 text-slate-300 font-mono">
              {events.length}
            </span>
          </div>

          <div className="flex items-center gap-2">
            <button
              onClick={handleDismissAll}
              className="text-xs text-slate-400 hover:text-white flex items-center gap-1 px-2 py-1 rounded hover:bg-slate-800 transition-colors"
              title="Clear all events"
            >
              <CheckCheck className="w-3.5 h-3.5" /> Clear
            </button>
            <button
              onClick={onClose}
              className="p-1 text-slate-400 hover:text-white rounded hover:bg-slate-800 transition-colors"
            >
              <X className="w-4 h-4" />
            </button>
          </div>
        </div>

        {/* Category Filters */}
        <div className="p-2.5 border-b border-slate-800/80 bg-slate-950/60 flex items-center gap-1.5 overflow-x-auto">
          {(['All', EventCategory.Economy, EventCategory.Government, EventCategory.Diplomacy, EventCategory.Military, EventCategory.Emergency] as const).map(
            (cat) => (
              <button
                key={cat}
                onClick={() => setSelectedCategory(cat)}
                className={`px-2 py-1 rounded text-[11px] font-semibold whitespace-nowrap transition-colors ${
                  selectedCategory === cat
                    ? 'bg-indigo-600 text-white'
                    : 'text-slate-400 hover:text-white hover:bg-slate-800/60'
                }`}
              >
                {cat}
              </button>
            )
          )}
        </div>

        {/* Events Feed */}
        <div className="flex-1 overflow-y-auto p-3 space-y-2.5">
          {reversedEvents.length === 0 ? (
            <div className="p-8 text-center text-xs text-slate-500 italic">
              No notifications recorded under this category.
            </div>
          ) : (
            reversedEvents.map((ev) => (
              <div
                key={ev.id}
                className="p-3 rounded-lg border border-slate-800/80 bg-slate-900/60 hover:border-slate-700 transition-colors space-y-1.5"
              >
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-1.5">
                    {getCategoryIcon(ev.category)}
                    <span className="text-xs font-bold text-white truncate max-w-[190px]">
                      {ev.title}
                    </span>
                  </div>
                  {getSeverityBadge(ev.severity)}
                </div>

                <p className="text-xs text-slate-300 leading-relaxed">{ev.message}</p>

                <div className="text-[10px] font-mono text-slate-500 flex items-center justify-between pt-1 border-t border-slate-800/50">
                  <span>{ev.date?.toString ? ev.date.toString() : 'Date'}</span>
                  <span className="uppercase text-slate-500">{ev.category}</span>
                </div>
              </div>
            ))
          )}
        </div>
      </div>
    </div>
  );
};
