import React from 'react';
import {
  TrendingUp,
  Landmark,
  Globe2,
  ShieldAlert,
  CheckSquare,
  Trophy,
  Save,
} from 'lucide-react';

export type ActivePanel =
  | 'none'
  | 'economy'
  | 'government'
  | 'diplomacy'
  | 'military'
  | 'tasks'
  | 'achievements'
  | 'saveLoad'
  | 'countryDetail'
  | 'notifications'
  | 'diagnostics';

interface BottomNavBarProps {
  activePanel: ActivePanel;
  onSelectPanel: (panel: ActivePanel) => void;
  completedTasksCount?: number;
  unlockedAchievementsCount?: number;
}

export const BottomNavBar: React.FC<BottomNavBarProps> = ({
  activePanel,
  onSelectPanel,
  completedTasksCount = 0,
  unlockedAchievementsCount = 0,
}) => {
  const navItems = [
    { id: 'economy' as ActivePanel, label: 'Economy & Budget', icon: TrendingUp },
    { id: 'government' as ActivePanel, label: 'Cabinet & Governance', icon: Landmark },
    { id: 'diplomacy' as ActivePanel, label: 'World Diplomacy', icon: Globe2 },
    { id: 'military' as ActivePanel, label: 'Armed Forces', icon: ShieldAlert },
    {
      id: 'tasks' as ActivePanel,
      label: 'Presidential Tasks',
      icon: CheckSquare,
      badge: completedTasksCount > 0 ? `${completedTasksCount}` : undefined,
    },
    {
      id: 'achievements' as ActivePanel,
      label: 'Achievements',
      icon: Trophy,
      badge: unlockedAchievementsCount > 0 ? `${unlockedAchievementsCount}` : undefined,
    },
    { id: 'saveLoad' as ActivePanel, label: 'Save & Load', icon: Save },
  ];

  return (
    <nav
      id="bottom-nav-bar"
      className="glass-panel"
      style={{
        position: 'absolute',
        bottom: '16px',
        left: '50%',
        transform: 'translateX(-50%)',
        zIndex: 50,
        display: 'flex',
        alignItems: 'center',
        gap: '8px',
        padding: '6px 10px',
        borderRadius: 'var(--radius-xl)',
        boxShadow: '0 10px 40px rgba(0, 0, 0, 0.6), 0 0 20px rgba(245, 158, 11, 0.1)',
        border: '1px solid rgba(255, 255, 255, 0.12)',
      }}
    >
      {navItems.map((item) => {
        const Icon = item.icon;
        const isActive = activePanel === item.id;

        return (
          <button
            key={item.id}
            onClick={() => onSelectPanel(isActive ? 'none' : item.id)}
            style={{
              position: 'relative',
              display: 'flex',
              alignItems: 'center',
              gap: '8px',
              padding: '8px 14px',
              borderRadius: 'var(--radius-md)',
              border: isActive ? '1px solid rgba(245, 158, 11, 0.5)' : '1px solid transparent',
              background: isActive
                ? 'linear-gradient(180deg, rgba(245, 158, 11, 0.2) 0%, rgba(245, 158, 11, 0.05) 100%)'
                : 'transparent',
              color: isActive ? '#fbbf24' : 'var(--text-muted)',
              fontSize: '13px',
              fontWeight: isActive ? 600 : 500,
              fontFamily: 'var(--font-sans)',
              cursor: 'pointer',
              transition: 'all 0.2s cubic-bezier(0.4, 0, 0.2, 1)',
              outline: 'none',
            }}
            onMouseEnter={(e) => {
              if (!isActive) {
                e.currentTarget.style.color = '#fff';
                e.currentTarget.style.background = 'rgba(255, 255, 255, 0.06)';
              }
            }}
            onMouseLeave={(e) => {
              if (!isActive) {
                e.currentTarget.style.color = 'var(--text-muted)';
                e.currentTarget.style.background = 'transparent';
              }
            }}
          >
            <Icon size={16} />
            <span>{item.label}</span>

            {item.badge && (
              <span
                style={{
                  background: 'var(--accent-gold)',
                  color: '#000',
                  fontSize: '10px',
                  fontWeight: 700,
                  borderRadius: '9999px',
                  padding: '1px 5px',
                  minWidth: '16px',
                  textAlign: 'center',
                }}
              >
                {item.badge}
              </span>
            )}
          </button>
        );
      })}
    </nav>
  );
};
