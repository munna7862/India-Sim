import React, { useState, useEffect, useRef, useCallback } from 'react';
import { GameSession } from './domain/models/GameSession';
import { SimulationEngine } from './domain/services/SimulationEngine';
import { SaveManager, LocalStorageSaveStorage } from './domain/services/SaveManager';
import { ClockSpeed } from './domain/models/types';
import { TopHUD } from './ui/components/TopHUD';
import { WorldMap } from './ui/components/WorldMap';
import { BottomNavBar, ActivePanel } from './ui/components/BottomNavBar';
import { EconomyPanel } from './ui/panels/EconomyPanel';
import { GovernmentPanel } from './ui/panels/GovernmentPanel';
import { DiplomacyPanel } from './ui/panels/DiplomacyPanel';
import { MilitaryPanel } from './ui/panels/MilitaryPanel';
import { TasksPanel } from './ui/panels/TasksPanel';
import { AchievementsPanel } from './ui/panels/AchievementsPanel';
import { SaveLoadModal } from './ui/components/SaveLoadModal';
import { CountryDetailModal } from './ui/components/CountryDetailModal';
import { DomesticEventModal } from './ui/components/DomesticEventModal';
import { EventLogDrawer } from './ui/components/EventLogDrawer';
import { AudioService } from './ui/services/AudioService';

export const App: React.FC = () => {
  // Master game session & engine instances
  const [session, setSession] = useState<GameSession>(() => GameSession.createInitialSession('IND'));
  const engineRef = useRef<SimulationEngine>(new SimulationEngine(session));
  const saveManagerRef = useRef<SaveManager>(new SaveManager(new LocalStorageSaveStorage()));

  // Active UI modal panel
  const [activePanel, setActivePanel] = useState<ActivePanel>('none');
  const [selectedMapCountryId, setSelectedMapCountryId] = useState<string | null>(null);
  const [isNotificationsDrawerOpen, setIsNotificationsDrawerOpen] = useState<boolean>(false);

  // Force re-render state trigger
  const [, setTickCounter] = useState<number>(0);
  const triggerUpdate = useCallback(() => {
    setTickCounter((prev) => prev + 1);
  }, []);

  // Update engine session reference whenever session changes (e.g. after load)
  useEffect(() => {
    engineRef.current.session = session;
    triggerUpdate();
  }, [session, triggerUpdate]);

  // Main simulation loop (requestAnimationFrame with clamped delta time)
  useEffect(() => {
    let animId: number;
    let lastTime = performance.now();
    let lastUiSync = performance.now();

    const loop = (currentTime: number) => {
      const dt = Math.min((currentTime - lastTime) / 1000, 0.1);
      lastTime = currentTime;

      const currentSession = engineRef.current.session;
      const hadPendingEvent = !!currentSession.pendingEvent;

      // Tick the simulation engine
      const tickResult = engineRef.current.tick(dt);

      // If an emergency crisis event just triggered, alert and pause
      if (!hadPendingEvent && currentSession.pendingEvent) {
        AudioService.playAlert();
        currentSession.clock.isPaused = true;
      }

      // Sync React state if date changed, month rolled over, or periodically (every 150ms)
      if (tickResult.monthRolledOver || tickResult.dateChanged || currentTime - lastUiSync > 150) {
        lastUiSync = currentTime;
        triggerUpdate();
      }

      animId = requestAnimationFrame(loop);
    };

    animId = requestAnimationFrame(loop);
    return () => cancelAnimationFrame(animId);
  }, [triggerUpdate]);

  // Keyboard shortcut handler
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      // Don't intercept when user is typing in an input
      if (['INPUT', 'SELECT', 'TEXTAREA'].includes((e.target as HTMLElement)?.tagName)) {
        return;
      }

      const s = engineRef.current.session;
      const clock = s.clock;

      switch (e.key) {
        case ' ':
          e.preventDefault();
          clock.isPaused = !clock.isPaused;
          AudioService.playClick();
          triggerUpdate();
          break;
        case '1':
          e.preventDefault();
          clock.speed = ClockSpeed.Normal;
          clock.isPaused = false;
          AudioService.playClick();
          triggerUpdate();
          break;
        case '2':
          e.preventDefault();
          clock.speed = ClockSpeed.Fast;
          clock.isPaused = false;
          AudioService.playClick();
          triggerUpdate();
          break;
        case '3':
          e.preventDefault();
          clock.speed = ClockSpeed.Ultra;
          clock.isPaused = false;
          AudioService.playClick();
          triggerUpdate();
          break;
        case 'F1':
          e.preventDefault();
          setActivePanel((prev) => (prev === 'government' ? 'none' : 'government'));
          AudioService.playClick();
          break;
        case 'F2':
          e.preventDefault();
          setActivePanel((prev) => (prev === 'tasks' ? 'none' : 'tasks'));
          AudioService.playClick();
          break;
        case 'F3':
          e.preventDefault();
          setIsNotificationsDrawerOpen((prev) => !prev);
          AudioService.playClick();
          break;
        case 'F4':
          e.preventDefault();
          setActivePanel((prev) => (prev === 'economy' ? 'none' : 'economy'));
          AudioService.playClick();
          break;
        case 'F5':
          e.preventDefault();
          saveManagerRef.current.quickSave(s);
          AudioService.playCoin();
          triggerUpdate();
          break;
        case 'F6':
          e.preventDefault();
          setActivePanel((prev) => (prev === 'diplomacy' ? 'none' : 'diplomacy'));
          AudioService.playClick();
          break;
        case 'F7':
          e.preventDefault();
          setActivePanel((prev) => (prev === 'military' ? 'none' : 'military'));
          AudioService.playClick();
          break;
        case 'F8':
          e.preventDefault();
          setActivePanel((prev) => (prev === 'achievements' ? 'none' : 'achievements'));
          AudioService.playClick();
          break;
        case 'F9':
          e.preventDefault();
          {
            const res = saveManagerRef.current.quickLoad();
            if (res.isSuccess && res.session) {
              setSession(res.session);
              AudioService.playFanfare();
            }
          }
          break;
        case 'Escape':
          e.preventDefault();
          setActivePanel('none');
          setSelectedMapCountryId(null);
          setIsNotificationsDrawerOpen(false);
          AudioService.playClick();
          break;
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [triggerUpdate]);

  // Handlers for TopHUD
  const handleSetSpeed = (spd: ClockSpeed) => {
    session.clock.speed = spd;
    session.clock.isPaused = false;
    AudioService.playClick();
    triggerUpdate();
  };

  const handleTogglePause = () => {
    session.clock.isPaused = !session.clock.isPaused;
    AudioService.playClick();
    triggerUpdate();
  };

  const handleQuickSave = () => {
    saveManagerRef.current.quickSave(session);
    AudioService.playCoin();
    triggerUpdate();
  };

  const handleSelectCountryOnMap = (cid: string) => {
    setSelectedMapCountryId(cid);
    AudioService.playClick();
  };

  const handleStartNewGame = () => {
    const fresh = GameSession.createInitialSession('IND');
    setSession(fresh);
    engineRef.current = new SimulationEngine(fresh);
    AudioService.playFanfare();
    triggerUpdate();
  };

  // Badge counters
  const completedTasksCount = Object.values(session.tasks).filter((t) => t.isRewardClaimed).length;
  const unlockedAchievementsCount = Object.values(session.achievements).filter((a) => a.isUnlocked).length;

  return (
    <div className="relative w-screen h-screen overflow-hidden bg-slate-950 select-none flex flex-col font-sans">
      {/* Top Geopolitical HUD */}
      <TopHUD
        session={session}
        onSetSpeed={handleSetSpeed}
        onTogglePause={handleTogglePause}
        onQuickSave={handleQuickSave}
        onOpenNotifications={() => setIsNotificationsDrawerOpen(true)}
        onOpenSettings={() => setActivePanel('saveLoad')}
        unreadNotificationsCount={session.events?.length ?? 0}
      />

      {/* Main Interactive Vector World Map */}
      <main className="relative flex-1 w-full h-full overflow-hidden">
        <WorldMap session={session} onSelectCountry={handleSelectCountryOnMap} />
      </main>

      {/* Bottom Dock Navigation Bar */}
      <BottomNavBar
        activePanel={activePanel}
        onSelectPanel={(p) => {
          setActivePanel((prev) => (prev === p ? 'none' : p));
          AudioService.playClick();
        }}
        completedTasksCount={completedTasksCount}
        unlockedAchievementsCount={unlockedAchievementsCount}
      />

      {/* --- MODALS & PANELS --- */}

      {/* F4: Economy Panel */}
      {activePanel === 'economy' && (
        <EconomyPanel
          session={session}
          engine={engineRef.current}
          onClose={() => setActivePanel('none')}
          onUpdate={triggerUpdate}
        />
      )}

      {/* F1: Government Panel */}
      {activePanel === 'government' && (
        <GovernmentPanel
          session={session}
          engine={engineRef.current}
          onClose={() => setActivePanel('none')}
          onUpdate={triggerUpdate}
        />
      )}

      {/* F6: Diplomacy Panel */}
      {activePanel === 'diplomacy' && (
        <DiplomacyPanel
          session={session}
          engine={engineRef.current}
          onClose={() => setActivePanel('none')}
          onUpdate={triggerUpdate}
          initialSelectedCountryId={selectedMapCountryId || undefined}
        />
      )}

      {/* F7: Military Panel */}
      {activePanel === 'military' && (
        <MilitaryPanel
          session={session}
          engine={engineRef.current}
          onClose={() => setActivePanel('none')}
          onUpdate={triggerUpdate}
        />
      )}

      {/* F2: Presidential Tasks Panel */}
      {activePanel === 'tasks' && (
        <TasksPanel
          session={session}
          engine={engineRef.current}
          onClose={() => setActivePanel('none')}
          onUpdate={triggerUpdate}
        />
      )}

      {/* F8: Achievements Panel */}
      {activePanel === 'achievements' && (
        <AchievementsPanel
          session={session}
          engine={engineRef.current}
          onClose={() => setActivePanel('none')}
          onUpdate={triggerUpdate}
        />
      )}

      {/* Save & Load Modal */}
      {activePanel === 'saveLoad' && (
        <SaveLoadModal
          session={session}
          saveManager={saveManagerRef.current}
          onClose={() => setActivePanel('none')}
          onLoadSession={(newSession) => {
            setSession(newSession);
            triggerUpdate();
          }}
          onNewGame={handleStartNewGame}
        />
      )}

      {/* Country Detail Modal (Triggered by clicking map) */}
      {selectedMapCountryId && (
        <CountryDetailModal
          session={session}
          countryId={selectedMapCountryId}
          onClose={() => setSelectedMapCountryId(null)}
          onOpenDiplomacy={() => {
            setSelectedMapCountryId(null);
            setActivePanel('diplomacy');
          }}
          onOpenMilitary={() => {
            setSelectedMapCountryId(null);
            setActivePanel('military');
          }}
        />
      )}

      {/* Flash Crisis Emergency Dialog */}
      {session.pendingEvent && (
        <DomesticEventModal
          session={session}
          engine={engineRef.current}
          onUpdate={triggerUpdate}
        />
      )}

      {/* Newsfeed Intelligence Wire Drawer */}
      <EventLogDrawer
        session={session}
        engine={engineRef.current}
        isOpen={isNotificationsDrawerOpen}
        onClose={() => setIsNotificationsDrawerOpen(false)}
        onUpdate={triggerUpdate}
      />
    </div>
  );
};
