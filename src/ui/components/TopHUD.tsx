import React from 'react';
import { GameSession } from '../../domain/models/GameSession';
import { ClockSpeed } from '../../domain/models/types';
import {
  Play,
  Pause,
  FastForward,
  Zap,
  Coins,
  TrendingUp,
  Shield,
  Heart,
  Smile,
  Wheat,
  Battery,
  Flame,
  Hammer,
  Radio,
  ShoppingBag,
  Bell,
  Save,
  Settings,
} from 'lucide-react';

interface TopHUDProps {
  session: GameSession;
  onSetSpeed: (speed: ClockSpeed) => void;
  onTogglePause: () => void;
  onQuickSave: () => void;
  onOpenNotifications: () => void;
  onOpenSettings: () => void;
  unreadNotificationsCount: number;
}

export const TopHUD: React.FC<TopHUDProps> = ({
  session,
  onSetSpeed,
  onTogglePause,
  onQuickSave,
  onOpenNotifications,
  onOpenSettings,
  unreadNotificationsCount,
}) => {
  const player = session.getPlayerCountry();
  if (!player) return null;

  const clock = session.clock;
  const isPaused = clock.isPaused || clock.speed === ClockSpeed.Paused;

  const formatMoney = (val: number): string => {
    const abs = Math.abs(val);
    const sign = val < 0 ? '-' : '';
    if (abs >= 1_000_000_000_000) return `${sign}$${(abs / 1_000_000_000_000).toFixed(2)}T`;
    if (abs >= 1_000_000_000) return `${sign}$${(abs / 1_000_000_000).toFixed(2)}B`;
    if (abs >= 1_000_000) return `${sign}$${(abs / 1_000_000).toFixed(1)}M`;
    return `${sign}$${abs.toLocaleString()}`;
  };

  const formatNumber = (val: number): string => {
    if (val >= 1_000_000) return `${(val / 1_000_000).toFixed(1)}M`;
    if (val >= 1_000) return `${(val / 1_000).toFixed(0)}k`;
    return val.toFixed(0);
  };

  const getRatingBadgeClass = (rating: string) => {
    if (rating === 'AAA') return 'badge-emerald';
    if (rating.startsWith('A')) return 'badge-blue';
    if (rating.startsWith('B')) return 'badge-gold';
    return 'badge-red';
  };

  return (
    <header
      id="top-hud"
      style={{
        position: 'absolute',
        top: 0,
        left: 0,
        right: 0,
        zIndex: 50,
        background: 'linear-gradient(180deg, rgba(7, 10, 18, 0.95) 0%, rgba(7, 10, 18, 0.8) 85%, transparent 100%)',
        backdropFilter: 'blur(12px)',
        borderBottom: '1px solid rgba(255, 255, 255, 0.08)',
        padding: '8px 16px',
        display: 'flex',
        flexDirection: 'column',
        gap: '6px',
      }}
    >
      {/* Top Row: Nation Flag, Date, Speed Controls, Quick Actions */}
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: '12px' }}>
        {/* Country Title & Flag */}
        <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
          <div
            style={{
              width: '32px',
              height: '22px',
              borderRadius: '3px',
              background: 'linear-gradient(180deg, #FF9933 33%, #FFFFFF 33%, #FFFFFF 66%, #128807 66%)',
              border: '1px solid rgba(255,255,255,0.2)',
              position: 'relative',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              boxShadow: '0 2px 6px rgba(0,0,0,0.5)',
            }}
          >
            <div
              style={{
                width: '6px',
                height: '6px',
                borderRadius: '50%',
                border: '1px solid #000080',
              }}
            />
          </div>
          <div>
            <div style={{ fontSize: '15px', fontWeight: 700, letterSpacing: '0.04em', color: '#fff' }}>
              {player.name}
            </div>
            <div style={{ fontSize: '11px', color: 'var(--text-muted)' }}>
              {player.ideology.replace(/([A-Z])/g, ' $1').trim()} • {player.creditRating} Rated
            </div>
          </div>
        </div>

        {/* Date & Simulation Speed */}
        <div
          style={{
            display: 'flex',
            alignItems: 'center',
            gap: '8px',
            background: 'rgba(15, 23, 42, 0.7)',
            padding: '4px 8px',
            borderRadius: 'var(--radius-md)',
            border: '1px solid var(--border-subtle)',
          }}
        >
          <div style={{ fontFamily: 'var(--font-mono)', fontSize: '14px', fontWeight: 600, minWidth: '100px', textAlign: 'center' }}>
            {clock.currentDate.toString()}
          </div>

          <div style={{ height: '18px', width: '1px', background: 'rgba(255,255,255,0.1)' }} />

          <button
            className={`btn ${isPaused ? 'btn-active' : ''}`}
            onClick={onTogglePause}
            title="Pause / Resume (Spacebar)"
            style={{ padding: '4px 8px', fontSize: '12px' }}
          >
            {isPaused ? <Play size={14} /> : <Pause size={14} />}
          </button>

          <button
            className={`btn ${clock.speed === ClockSpeed.Normal && !isPaused ? 'btn-active' : ''}`}
            onClick={() => onSetSpeed(ClockSpeed.Normal)}
            title="Normal Speed (1x)"
            style={{ padding: '4px 8px', fontSize: '12px', fontFamily: 'var(--font-mono)' }}
          >
            1x
          </button>

          <button
            className={`btn ${clock.speed === ClockSpeed.Fast && !isPaused ? 'btn-active' : ''}`}
            onClick={() => onSetSpeed(ClockSpeed.Fast)}
            title="Fast Speed (2x)"
            style={{ padding: '4px 8px', fontSize: '12px', fontFamily: 'var(--font-mono)' }}
          >
            <FastForward size={14} />
          </button>

          <button
            className={`btn ${clock.speed === ClockSpeed.Ultra && !isPaused ? 'btn-active' : ''}`}
            onClick={() => onSetSpeed(ClockSpeed.Ultra)}
            title="Ultra Speed (4x)"
            style={{ padding: '4px 8px', fontSize: '12px', fontFamily: 'var(--font-mono)' }}
          >
            <Zap size={14} />
          </button>
        </div>

        {/* Vital Indicators: Stability, Approval, Quality of Living */}
        <div style={{ display: 'flex', alignItems: 'center', gap: '14px' }}>
          {/* Stability */}
          <div style={{ display: 'flex', alignItems: 'center', gap: '6px' }} title={`National Stability: ${player.stability.toFixed(1)}%`}>
            <Shield size={16} color="var(--accent-cyan)" />
            <div style={{ width: '48px', height: '6px', background: 'rgba(255,255,255,0.1)', borderRadius: '3px', overflow: 'hidden' }}>
              <div style={{ width: `${player.stability}%`, height: '100%', background: 'var(--accent-cyan)', transition: 'width 0.3s ease' }} />
            </div>
            <span style={{ fontSize: '12px', fontWeight: 600, color: 'var(--accent-cyan)' }}>{player.stability.toFixed(0)}%</span>
          </div>

          {/* Approval */}
          <div style={{ display: 'flex', alignItems: 'center', gap: '6px' }} title={`Public Approval: ${player.approvalRating.toFixed(1)}%`}>
            <Smile size={16} color="var(--accent-gold)" />
            <div style={{ width: '48px', height: '6px', background: 'rgba(255,255,255,0.1)', borderRadius: '3px', overflow: 'hidden' }}>
              <div style={{ width: `${player.approvalRating}%`, height: '100%', background: 'var(--accent-gold)', transition: 'width 0.3s ease' }} />
            </div>
            <span style={{ fontSize: '12px', fontWeight: 600, color: 'var(--accent-gold)' }}>{player.approvalRating.toFixed(0)}%</span>
          </div>

          {/* Quality of Living */}
          <div style={{ display: 'flex', alignItems: 'center', gap: '6px' }} title={`Quality of Living: ${player.qualityOfLiving.toFixed(1)}%`}>
            <Heart size={16} color="var(--accent-emerald)" />
            <div style={{ width: '48px', height: '6px', background: 'rgba(255,255,255,0.1)', borderRadius: '3px', overflow: 'hidden' }}>
              <div style={{ width: `${player.qualityOfLiving}%`, height: '100%', background: 'var(--accent-emerald)', transition: 'width 0.3s ease' }} />
            </div>
            <span style={{ fontSize: '12px', fontWeight: 600, color: 'var(--accent-emerald)' }}>{player.qualityOfLiving.toFixed(0)}%</span>
          </div>
        </div>

        {/* Quick Save, Notifications, Settings */}
        <div style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
          <button className="btn" onClick={onQuickSave} title="Quick Save (F5)" style={{ padding: '6px 10px', fontSize: '12px' }}>
            <Save size={14} />
            <span style={{ fontSize: '11px', opacity: 0.8 }}>F5</span>
          </button>

          <button
            className="btn"
            onClick={onOpenNotifications}
            title="Event Notifications"
            style={{ position: 'relative', padding: '6px 10px' }}
          >
            <Bell size={15} />
            {unreadNotificationsCount > 0 && (
              <span
                style={{
                  position: 'absolute',
                  top: '-4px',
                  right: '-4px',
                  background: 'var(--accent-rose)',
                  color: '#fff',
                  fontSize: '10px',
                  fontWeight: 700,
                  borderRadius: '9999px',
                  padding: '1px 5px',
                  minWidth: '16px',
                  textAlign: 'center',
                  boxShadow: '0 0 8px rgba(244, 63, 94, 0.7)',
                }}
              >
                {unreadNotificationsCount}
              </span>
            )}
          </button>

          <button className="btn" onClick={onOpenSettings} title="Settings & Save Slots" style={{ padding: '6px 10px' }}>
            <Settings size={15} />
          </button>
        </div>
      </div>

      {/* Second Row: Live Macro Ticker & Strategic Stockpiles */}
      <div
        style={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          fontSize: '12px',
          paddingTop: '4px',
          borderTop: '1px solid rgba(255, 255, 255, 0.05)',
        }}
      >
        {/* Treasury, GDP, Debt, Credit */}
        <div style={{ display: 'flex', alignItems: 'center', gap: '16px' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '5px' }} title="National Liquid Treasury">
            <Coins size={14} color="#fbbf24" />
            <span style={{ color: 'var(--text-muted)' }}>Treasury:</span>
            <span style={{ fontWeight: 600, color: player.treasury >= 0 ? '#34d399' : '#f87171', fontFamily: 'var(--font-mono)' }}>
              {formatMoney(player.treasury)}
            </span>
          </div>

          <div style={{ display: 'flex', alignItems: 'center', gap: '5px' }} title="Monthly Gross Domestic Product">
            <TrendingUp size={14} color="#60a5fa" />
            <span style={{ color: 'var(--text-muted)' }}>GDP/mo:</span>
            <span style={{ fontWeight: 600, fontFamily: 'var(--font-mono)' }}>
              {formatMoney(player.calculateMonthlyGdp())}
            </span>
          </div>

          <div style={{ display: 'flex', alignItems: 'center', gap: '5px' }} title="Outstanding Sovereign Debt">
            <span style={{ color: 'var(--text-muted)' }}>Debt:</span>
            <span style={{ fontWeight: 600, color: player.debt > 0 ? '#f87171' : 'var(--text-muted)', fontFamily: 'var(--font-mono)' }}>
              {formatMoney(player.debt)}
            </span>
          </div>

          <div style={{ display: 'flex', alignItems: 'center', gap: '5px' }} title="Sovereign Credit Rating">
            <span style={{ color: 'var(--text-muted)' }}>Rating:</span>
            <span className={`badge ${getRatingBadgeClass(player.creditRating)}`}>{player.creditRating}</span>
          </div>
        </div>

        {/* Strategic Stockpiles */}
        <div style={{ display: 'flex', alignItems: 'center', gap: '14px', fontFamily: 'var(--font-mono)' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '4px' }} title="Food Stockpile">
            <Wheat size={13} color="#f59e0b" />
            <span>{formatNumber(player.stockpiles.food)}</span>
          </div>

          <div style={{ display: 'flex', alignItems: 'center', gap: '4px' }} title="Energy Reserves">
            <Battery size={13} color="#06b6d4" />
            <span>{formatNumber(player.stockpiles.energy)}</span>
          </div>

          <div style={{ display: 'flex', alignItems: 'center', gap: '4px' }} title="Strategic Crude Oil">
            <Flame size={13} color="#ef4444" />
            <span>{formatNumber(player.stockpiles.oil)}</span>
          </div>

          <div style={{ display: 'flex', alignItems: 'center', gap: '4px' }} title="Iron & Industrial Ore">
            <Hammer size={13} color="#94a3b8" />
            <span>{formatNumber(player.stockpiles.iron)}</span>
          </div>

          <div style={{ display: 'flex', alignItems: 'center', gap: '4px' }} title="Refined Uranium Stockpile">
            <Radio size={13} color="#10b981" />
            <span>{formatNumber(player.stockpiles.uranium)}</span>
          </div>

          <div style={{ display: 'flex', alignItems: 'center', gap: '4px' }} title="Consumer Goods">
            <ShoppingBag size={13} color="#a855f7" />
            <span>{formatNumber(player.stockpiles.consumerGoods)}</span>
          </div>
        </div>
      </div>
    </header>
  );
};
