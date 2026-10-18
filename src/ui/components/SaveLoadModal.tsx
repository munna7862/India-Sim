import React, { useState, useEffect } from 'react';
import { GameSession } from '../../domain/models/GameSession';
import { SaveManager } from '../../domain/services/SaveManager';
import { SaveSlotMetadata } from '../../domain/services/SaveModels';
import {
  Save,
  Download,
  Upload,
  Trash2,
  X,
  AlertTriangle,
  Check,
  RotateCcw,
  HardDrive,
} from 'lucide-react';

interface SaveLoadModalProps {
  session: GameSession;
  saveManager: SaveManager;
  onClose: () => void;
  onLoadSession: (newSession: GameSession) => void;
  onNewGame?: () => void;
}

export const SaveLoadModal: React.FC<SaveLoadModalProps> = ({
  session,
  saveManager,
  onClose,
  onLoadSession,
  onNewGame,
}) => {
  const [slots, setSlots] = useState<Record<string, SaveSlotMetadata | null>>({});
  const [feedback, setFeedback] = useState<{ msg: string; isError: boolean } | null>(null);

  const slotIds = ['slot_1', 'slot_2', 'slot_3', 'slot_4', 'slot_5', 'quicksave', 'autosave'];

  const reloadSlots = () => {
    const updated: Record<string, SaveSlotMetadata | null> = {};
    for (const sid of slotIds) {
      updated[sid] = saveManager.getMetadata(sid);
    }
    setSlots(updated);
  };

  useEffect(() => {
    reloadSlots();
  }, []);

  const showMsg = (msg: string, isError: boolean = false) => {
    setFeedback({ msg, isError });
    setTimeout(() => setFeedback(null), 3500);
  };

  const handleSaveSlot = (slotId: string) => {
    const res = saveManager.save(slotId, session);
    if (res.isSuccess) {
      showMsg(`Successfully saved state to ${slotId.toUpperCase()}`);
      reloadSlots();
    } else {
      showMsg(res.errorMessage || 'Save operation failed', true);
    }
  };

  const handleLoadSlot = (slotId: string) => {
    const res = saveManager.load(slotId);
    if (res.isSuccess && res.session) {
      showMsg(`Successfully loaded ${slotId.toUpperCase()}`);
      onLoadSession(res.session);
      onClose();
    } else {
      showMsg(res.errorMessage || 'Load operation failed', true);
    }
  };

  const handleDeleteSlot = (slotId: string) => {
    if (confirm(`Are you sure you want to delete ${slotId}?`)) {
      saveManager.delete(slotId);
      showMsg(`Deleted ${slotId}`);
      reloadSlots();
    }
  };

  const handleExportJson = () => {
    try {
      const json = saveManager.serializeSession(session);
      const blob = new Blob([json], { type: 'application/json' });
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `IndiaSim_Save_${session.clock.currentDate.toString().replace(/-/g, '')}.json`;
      a.click();
      URL.revokeObjectURL(url);
      showMsg('Session exported to JSON successfully.');
    } catch (err: any) {
      showMsg(`Export failed: ${err.message}`, true);
    }
  };

  const handleImportJson = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    const reader = new FileReader();
    reader.onload = (event) => {
      try {
        const text = event.target?.result as string;
        // Save imported text directly to slot_1 or a temporary slot
        (saveManager as any).storage.setItem('imported', text);
        const loadRes = saveManager.load('imported');
        if (loadRes.isSuccess && loadRes.session) {
          showMsg('Imported save file successfully loaded!');
          onLoadSession(loadRes.session);
          onClose();
        } else {
          showMsg(loadRes.errorMessage || 'Invalid or corrupted save file', true);
        }
      } catch (err: any) {
        showMsg(`Failed to import file: ${err.message}`, true);
      }
    };
    reader.readAsText(file);
  };

  return (
    <div className="panel-overlay" onClick={onClose}>
      <div
        className="glass-card panel-container max-w-3xl w-full max-h-[90vh] flex flex-col overflow-hidden"
        onClick={(e) => e.stopPropagation()}
      >
        {/* Header */}
        <div className="p-4 border-b border-slate-700/60 flex items-center justify-between bg-slate-900/60">
          <div className="flex items-center gap-3">
            <Save className="w-6 h-6 text-indigo-400" />
            <div>
              <h2 className="text-xl font-bold text-white tracking-wide">State Persistence & Game Archives</h2>
              <p className="text-xs text-slate-400">Save, restore, export, and manage simulation states</p>
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

        {/* Utility Actions Bar */}
        <div className="p-4 border-b border-slate-800/80 bg-slate-950/40 flex flex-wrap items-center justify-between gap-3">
          <div className="flex items-center gap-2">
            <button
              onClick={handleExportJson}
              className="btn btn-neutral text-xs flex items-center gap-1.5 text-slate-300 hover:text-white"
            >
              <Download className="w-3.5 h-3.5 text-indigo-400" /> Export JSON
            </button>
            <label className="btn btn-neutral text-xs flex items-center gap-1.5 text-slate-300 hover:text-white cursor-pointer">
              <Upload className="w-3.5 h-3.5 text-cyan-400" /> Import JSON
              <input
                type="file"
                accept=".json"
                onChange={handleImportJson}
                className="hidden"
              />
            </label>
          </div>

          {onNewGame && (
            <button
              onClick={() => {
                if (confirm('Start a new simulation session? Unsaved progress will be discarded.')) {
                  onNewGame();
                  onClose();
                }
              }}
              className="btn btn-danger text-xs flex items-center gap-1.5"
            >
              <RotateCcw className="w-3.5 h-3.5" /> Start New Simulation
            </button>
          )}
        </div>

        {/* Slots List */}
        <div className="flex-1 overflow-y-auto p-4 space-y-3">
          {slotIds.map((slotId) => {
            const meta = slots[slotId];
            const isSpecial = slotId === 'quicksave' || slotId === 'autosave';

            return (
              <div
                key={slotId}
                className="glass-panel p-3.5 border border-slate-800 flex flex-col md:flex-row md:items-center justify-between gap-3 hover:border-slate-700 transition-colors"
              >
                <div>
                  <div className="flex items-center gap-2 mb-1">
                    <span className="font-mono text-xs font-bold text-indigo-300 px-2 py-0.5 rounded bg-slate-800 uppercase">
                      {slotId.replace('_', ' ')}
                    </span>
                    {isSpecial && (
                      <span className="text-[10px] text-amber-400 font-mono">
                        {slotId === 'quicksave' ? '(Hotkey: F5 Save / F9 Load)' : '(Automatic)'}
                      </span>
                    )}
                  </div>

                  {meta ? (
                    <div className="text-xs text-slate-400 space-y-0.5">
                      <div className="text-white font-semibold">
                        {meta.countryName} - Date: {meta.inGameDate?.toString ? meta.inGameDate.toString() : 'Unknown'}
                      </div>
                      <div className="text-[11px] text-slate-500 flex items-center gap-3">
                        <span>Saved: {new Date(meta.realTimeSavedUtc).toLocaleString()}</span>
                        <span>Size: {(meta.fileSizeBytes / 1024).toFixed(1)} KB</span>
                      </div>
                    </div>
                  ) : (
                    <div className="text-xs text-slate-500 italic">Empty Slot</div>
                  )}
                </div>

                {/* Actions */}
                <div className="flex items-center gap-2 shrink-0">
                  <button
                    onClick={() => handleSaveSlot(slotId)}
                    className="btn btn-primary bg-indigo-600 hover:bg-indigo-500 text-xs py-1.5 px-3 flex items-center gap-1"
                  >
                    <Save className="w-3.5 h-3.5" /> Overwrite
                  </button>

                  <button
                    onClick={() => handleLoadSlot(slotId)}
                    disabled={!meta}
                    className="btn btn-neutral text-xs py-1.5 px-3 flex items-center gap-1 disabled:opacity-40"
                  >
                    <HardDrive className="w-3.5 h-3.5" /> Restore
                  </button>

                  {meta && !isSpecial && (
                    <button
                      onClick={() => handleDeleteSlot(slotId)}
                      className="p-1.5 text-rose-400 hover:text-rose-300 hover:bg-rose-950/60 rounded-lg transition-colors"
                      title="Delete save slot"
                    >
                      <Trash2 className="w-4 h-4" />
                    </button>
                  )}
                </div>
              </div>
            );
          })}
        </div>
      </div>
    </div>
  );
};
