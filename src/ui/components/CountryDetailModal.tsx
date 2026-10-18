import React from 'react';
import { GameSession } from '../../domain/models/GameSession';
import {
  Globe2,
  X,
  Swords,
  Shield,
  Coins,
  TrendingUp,
  HeartHandshake,
  Users,
} from 'lucide-react';

interface CountryDetailModalProps {
  session: GameSession;
  countryId: string | null;
  onClose: () => void;
  onOpenDiplomacy: (countryId: string) => void;
  onOpenMilitary: () => void;
}

export const CountryDetailModal: React.FC<CountryDetailModalProps> = ({
  session,
  countryId,
  onClose,
  onOpenDiplomacy,
  onOpenMilitary,
}) => {
  if (!countryId) return null;
  const country = session.countries[countryId];
  const player = session.getPlayerCountry();
  if (!country || !player) return null;

  const isPlayer = country.id === player.id;
  const stance = isPlayer ? null : player.getRelationsWith(country.id);

  const getRelationsColor = (rel: number) => {
    if (rel >= 60) return 'text-emerald-400';
    if (rel >= 20) return 'text-green-400';
    if (rel >= -20) return 'text-slate-300';
    if (rel >= -60) return 'text-amber-400';
    return 'text-rose-400';
  };

  return (
    <div className="panel-overlay z-40 flex items-center justify-center p-4" onClick={onClose}>
      <div
        className="glass-card panel-container max-w-lg w-full overflow-hidden animate-in fade-in zoom-in-95 duration-200"
        onClick={(e) => e.stopPropagation()}
      >
        {/* Header */}
        <div className="p-4 border-b border-slate-700/60 flex items-center justify-between bg-slate-900/60">
          <div className="flex items-center gap-3">
            <Globe2 className="w-6 h-6 text-indigo-400" />
            <div>
              <div className="flex items-center gap-2">
                <h3 className="text-xl font-bold text-white">{country.name}</h3>
                <span className="font-mono text-xs px-2 py-0.5 rounded bg-slate-800 text-indigo-300 font-bold">
                  {country.id}
                </span>
                {isPlayer && (
                  <span className="badge badge-success text-[10px]">Your Sovereign Homeland</span>
                )}
              </div>
              <p className="text-xs text-slate-400 mt-0.5">Sovereign Geopolitical Intelligence Dossier</p>
            </div>
          </div>
          <button
            onClick={onClose}
            className="p-1.5 text-slate-400 hover:text-white rounded-lg hover:bg-slate-800/80 transition-colors"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Stats Grid */}
        <div className="p-5 space-y-4">
          <div className="grid grid-cols-2 gap-3">
            <div className="glass-panel p-3">
              <div className="text-xs text-slate-400 flex items-center gap-1.5">
                <Users className="w-3.5 h-3.5 text-cyan-400" /> Population
              </div>
              <div className="text-lg font-bold text-white font-mono mt-1">
                {(country.population / 1_000_000).toFixed(1)}M
              </div>
            </div>

            <div className="glass-panel p-3">
              <div className="text-xs text-slate-400 flex items-center gap-1.5">
                <TrendingUp className="w-3.5 h-3.5 text-emerald-400" /> Nominal GDP
              </div>
              <div className="text-lg font-bold text-white font-mono mt-1">
                ${(country.gdp / 1_000_000_000).toFixed(1)}B
              </div>
            </div>

            <div className="glass-panel p-3">
              <div className="text-xs text-slate-400 flex items-center gap-1.5">
                <Coins className="w-3.5 h-3.5 text-amber-400" /> Treasury Reserves
              </div>
              <div className="text-lg font-bold text-white font-mono mt-1">
                ${(country.treasury / 1_000_000_000).toFixed(1)}B
              </div>
            </div>

            <div className="glass-panel p-3">
              <div className="text-xs text-slate-400 flex items-center gap-1.5">
                <Shield className="w-3.5 h-3.5 text-rose-400" /> Stability / Approval
              </div>
              <div className="text-lg font-bold text-white font-mono mt-1">
                {country.stability.toFixed(0)}% / {country.approvalRating.toFixed(0)}%
              </div>
            </div>
          </div>

          {/* Stance towards player */}
          {stance && (
            <div className="glass-panel p-3.5 border border-slate-800">
              <div className="text-xs font-mono uppercase text-slate-400 mb-2">Bilateral Diplomatic Stance</div>
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-2">
                  {stance.isAtWar ? (
                    <span className="badge badge-danger text-xs flex items-center gap-1">
                      <Swords className="w-3.5 h-3.5" /> State of War
                    </span>
                  ) : stance.hasNonAggressionPact ? (
                    <span className="badge badge-success text-xs flex items-center gap-1">
                      <Shield className="w-3.5 h-3.5" /> Non-Aggression Pact
                    </span>
                  ) : (
                    <span className="badge badge-neutral text-xs">Peaceful Ties</span>
                  )}
                </div>

                <div className="flex items-center gap-2">
                  <span className="text-xs text-slate-400">Opinion:</span>
                  <span className={`font-mono text-base font-bold ${getRelationsColor(stance.relations)}`}>
                    {stance.relations > 0 ? `+${stance.relations.toFixed(0)}` : stance.relations.toFixed(0)}
                  </span>
                </div>
              </div>
            </div>
          )}

          {/* Action Triggers */}
          {!isPlayer && (
            <div className="flex items-center gap-3 pt-2">
              <button
                onClick={() => {
                  onClose();
                  onOpenDiplomacy(country.id);
                }}
                className="btn btn-primary bg-indigo-600 hover:bg-indigo-500 text-xs flex-1 flex items-center justify-center gap-2 py-2"
              >
                <HeartHandshake className="w-4 h-4" /> Open Bilateral Diplomacy
              </button>

              {stance?.isAtWar && (
                <button
                  onClick={() => {
                    onClose();
                    onOpenMilitary();
                  }}
                  className="btn btn-danger text-xs flex-1 flex items-center justify-center gap-2 py-2"
                >
                  <Swords className="w-4 h-4" /> Military Frontline
                </button>
              )}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};
