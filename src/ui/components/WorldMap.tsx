import React, { useState, useEffect, useRef } from 'react';
import { geoNaturalEarth1, geoPath, geoGraticule } from 'd3-geo';
import { GameSession } from '../../domain/models/GameSession';
import { CountryState } from '../../domain/models/CountryState';
import { ZoomIn, ZoomOut, RotateCcw, Handshake, Swords } from 'lucide-react';

interface WorldMapProps {
  session: GameSession;
  onSelectCountry: (countryId: string) => void;
}

interface HoveredCountryInfo {
  id: string;
  name: string;
  capital: string;
  countryState?: CountryState;
  relations: number;
  isAtWar: boolean;
  hasPact: boolean;
  activeContractsCount: number;
  statusText: string;
  x: number;
  y: number;
}

export const WorldMap: React.FC<WorldMapProps> = ({ session, onSelectCountry }) => {
  const [geoData, setGeoData] = useState<any>(null);
  const [transform, setTransform] = useState({ k: 1, x: 0, y: 0 });
  const [hovered, setHovered] = useState<HoveredCountryInfo | null>(null);
  const [isDragging, setIsDragging] = useState(false);
  const [dragStart, setDragStart] = useState({ x: 0, y: 0 });
  const containerRef = useRef<HTMLDivElement>(null);

  // Load GeoJSON data
  useEffect(() => {
    fetch('./content/map/world_countries.geojson')
      .then((res) => {
        if (!res.ok) throw new Error('Failed to fetch from ./content/map/world_countries.geojson');
        return res.json();
      })
      .then((data) => setGeoData(data))
      .catch((err) => {
        console.warn('Fallback fetching /content/map/world_countries.geojson', err);
        fetch('/content/map/world_countries.geojson')
          .then((r) => r.json())
          .then((d) => setGeoData(d))
          .catch((e) => console.error('Failed to load GeoJSON:', e));
      });
  }, []);

  const width = 1000;
  const height = 520;

  // Setup projection
  const projection = geoNaturalEarth1()
    .scale(175)
    .translate([width / 2, height / 2]);

  const pathGenerator = geoPath().projection(projection);
  const graticuleGenerator = geoGraticule();

  const player = session.getPlayerCountry();

  // Mouse wheel zoom
  const handleWheel = (e: React.WheelEvent) => {
    e.preventDefault();
    const zoomFactor = e.deltaY < 0 ? 1.15 : 0.85;
    setTransform((prev) => {
      const newK = Math.max(0.8, Math.min(6.0, prev.k * zoomFactor));
      return { ...prev, k: newK };
    });
  };

  // Drag pan
  const handleMouseDown = (e: React.MouseEvent) => {
    if (e.button !== 0) return; // Left mouse button only
    setIsDragging(true);
    setDragStart({ x: e.clientX - transform.x, y: e.clientY - transform.y });
  };

  const handleMouseMove = (e: React.MouseEvent) => {
    if (isDragging) {
      setTransform((prev) => ({
        ...prev,
        x: e.clientX - dragStart.x,
        y: e.clientY - dragStart.y,
      }));
    }
  };

  const handleMouseUp = () => {
    setIsDragging(false);
  };

  const resetZoom = () => {
    setTransform({ k: 1, x: 0, y: 0 });
  };

  // Country fill styling according to geopolitical stance
  const getCountryStyle = (feature: any) => {
    const id = feature.properties?.id || feature.properties?.iso_a3;
    if (!id) return { fill: '#334155', stroke: '#1e293b' };

    if (id === session.playerCountryId) {
      return {
        fill: '#059669', // Emerald protagonist
        stroke: '#f59e0b', // Gold border
        strokeWidth: 2,
        filter: 'drop-shadow(0 0 10px rgba(16, 185, 129, 0.4))',
      };
    }

    const stance = player ? player.getRelationsWith(id) : null;

    if (!stance) {
      return { fill: '#1e293b', stroke: '#0f172a', strokeWidth: 1 };
    }

    if (stance.isAtWar) {
      return {
        fill: '#b91c1c', // Deep crimson
        stroke: '#ef4444',
        strokeWidth: 1.5,
      };
    }

    if (stance.hasAlliance || stance.relations >= 60.0) {
      return {
        fill: '#1d4ed8', // Strategic Ally Blue
        stroke: '#60a5fa',
        strokeWidth: 1.2,
      };
    }

    if (stance.relations >= 20.0) {
      return {
        fill: '#0284c7', // Friendly Cyan
        stroke: '#38bdf8',
        strokeWidth: 1,
      };
    }

    if (stance.relations >= 0.0) {
      return {
        fill: '#475569', // Neutral Slate
        stroke: '#64748b',
        strokeWidth: 1,
      };
    }

    if (stance.relations >= -40.0) {
      return {
        fill: '#c2410c', // Cold Orange
        stroke: '#fb923c',
        strokeWidth: 1,
      };
    }

    return {
      fill: '#991b1b', // Hostile Red
      stroke: '#f87171',
      strokeWidth: 1.2,
    };
  };

  const handleCountryHover = (feature: any, e: React.MouseEvent) => {
    const id = feature.properties?.id || feature.properties?.iso_a3;
    const name = feature.properties?.name || id;
    const capital = feature.properties?.capital || 'Capital City';
    const countryState = session.countries[id];
    const stance = player ? player.getRelationsWith(id) : null;

    const rect = containerRef.current?.getBoundingClientRect();
    const x = e.clientX - (rect?.left || 0);
    const y = e.clientY - (rect?.top || 0);

    setHovered({
      id,
      name,
      capital,
      countryState,
      relations: stance ? stance.relations : 0,
      isAtWar: stance ? stance.isAtWar : false,
      hasPact: stance ? stance.hasActiveNonAggressionPact(session.clock.currentDate) : false,
      activeContractsCount: stance ? stance.activeTradeContracts.length : 0,
      statusText: stance ? stance.getStatusText() : 'Sovereign Republic',
      x,
      y,
    });
  };

  return (
    <div
      ref={containerRef}
      id="world-map-viewport"
      onWheel={handleWheel}
      onMouseDown={handleMouseDown}
      onMouseMove={handleMouseMove}
      onMouseUp={handleMouseUp}
      style={{
        position: 'absolute',
        top: 0,
        left: 0,
        right: 0,
        bottom: 0,
        overflow: 'hidden',
        background: 'radial-gradient(ellipse at 50% 50%, #0c1529 0%, #070a12 100%)',
        cursor: isDragging ? 'grabbing' : 'grab',
        userSelect: 'none',
      }}
    >
      {/* Zoom / Navigation controls in bottom right */}
      <div
        style={{
          position: 'absolute',
          bottom: '80px',
          right: '20px',
          zIndex: 40,
          display: 'flex',
          flexDirection: 'column',
          gap: '6px',
        }}
      >
        <button
          className="btn"
          onClick={() => setTransform((p) => ({ ...p, k: Math.min(6, p.k * 1.3) }))}
          title="Zoom In (+)"
          style={{ padding: '8px' }}
        >
          <ZoomIn size={16} />
        </button>
        <button
          className="btn"
          onClick={() => setTransform((p) => ({ ...p, k: Math.max(0.8, p.k * 0.77) }))}
          title="Zoom Out (-)"
          style={{ padding: '8px' }}
        >
          <ZoomOut size={16} />
        </button>
        <button className="btn" onClick={resetZoom} title="Reset Map View" style={{ padding: '8px' }}>
          <RotateCcw size={16} />
        </button>
      </div>

      {/* SVG Canvas */}
      <svg
        width="100%"
        height="100%"
        viewBox={`0 0 ${width} ${height}`}
        style={{ width: '100%', height: '100%', display: 'block' }}
      >
        <g transform={`translate(${transform.x}, ${transform.y}) scale(${transform.k})`}>
          {/* Subtle ocean grid lines */}
          <path
            d={pathGenerator(graticuleGenerator()) || ''}
            fill="none"
            stroke="rgba(255, 255, 255, 0.03)"
            strokeWidth="0.5"
          />

          {/* Countries */}
          {geoData &&
            geoData.features.map((feature: any, idx: number) => {
              const id = feature.properties?.id || feature.properties?.iso_a3;
              const isSelected = hovered?.id === id;
              const style = getCountryStyle(feature);

              return (
                <path
                  key={id || idx}
                  d={pathGenerator(feature) || ''}
                  fill={isSelected ? '#38bdf8' : style.fill}
                  stroke={isSelected ? '#ffffff' : style.stroke}
                  strokeWidth={isSelected ? 2 : style.strokeWidth}
                  style={{
                    transition: 'fill 0.2s ease, stroke 0.2s ease, filter 0.2s ease',
                    cursor: 'pointer',
                  }}
                  onMouseEnter={(e) => handleCountryHover(feature, e)}
                  onMouseLeave={() => setHovered(null)}
                  onClick={(e) => {
                    e.stopPropagation();
                    if (id) onSelectCountry(id);
                  }}
                />
              );
            })}
        </g>
      </svg>

      {/* Hover Inspection Tooltip */}
      {hovered && (
        <div
          className="glass-panel"
          style={{
            position: 'absolute',
            left: `${Math.min(window.innerWidth - 260, hovered.x + 16)}px`,
            top: `${Math.min(window.innerHeight - 200, hovered.y + 16)}px`,
            zIndex: 60,
            pointerEvents: 'none',
            padding: '12px',
            minWidth: '220px',
            border: '1px solid rgba(255, 255, 255, 0.15)',
          }}
        >
          <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: '4px' }}>
            <span style={{ fontSize: '15px', fontWeight: 700, color: '#fff' }}>{hovered.name}</span>
            <span className="badge badge-gold">{hovered.id}</span>
          </div>

          <div style={{ fontSize: '11px', color: 'var(--text-muted)', marginBottom: '8px' }}>
            Capital: {hovered.capital}
          </div>

          {hovered.id === session.playerCountryId ? (
            <div style={{ color: 'var(--accent-emerald)', fontSize: '12px', fontWeight: 600 }}>
              ★ Sovereign Protagonist Nation
            </div>
          ) : (
            <div style={{ display: 'flex', flexDirection: 'column', gap: '4px', fontSize: '12px' }}>
              <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                <span style={{ color: 'var(--text-muted)' }}>Status:</span>
                <span
                  style={{
                    fontWeight: 600,
                    color: hovered.isAtWar
                      ? 'var(--accent-red)'
                      : hovered.relations >= 20
                      ? 'var(--accent-cyan)'
                      : hovered.relations < 0
                      ? 'var(--accent-rose)'
                      : 'var(--text-main)',
                  }}
                >
                  {hovered.statusText}
                </span>
              </div>

              <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                <span style={{ color: 'var(--text-muted)' }}>Relations:</span>
                <span
                  style={{
                    fontFamily: 'var(--font-mono)',
                    fontWeight: 600,
                    color: hovered.relations >= 0 ? '#34d399' : '#f87171',
                  }}
                >
                  {hovered.relations >= 0 ? `+${hovered.relations.toFixed(0)}` : hovered.relations.toFixed(0)}
                </span>
              </div>

              {hovered.hasPact && (
                <div style={{ display: 'flex', alignItems: 'center', gap: '4px', color: 'var(--accent-emerald)', fontSize: '11px' }}>
                  <Handshake size={12} /> Non-Aggression Pact Active
                </div>
              )}

              {hovered.isAtWar && (
                <div style={{ display: 'flex', alignItems: 'center', gap: '4px', color: 'var(--accent-red)', fontSize: '11px' }}>
                  <Swords size={12} /> State of Active War
                </div>
              )}

              {hovered.countryState && (
                <div style={{ display: 'flex', justifyContent: 'space-between', borderTop: '1px solid rgba(255,255,255,0.06)', paddingTop: '4px', marginTop: '4px' }}>
                  <span style={{ color: 'var(--text-muted)' }}>Military Units:</span>
                  <span style={{ fontFamily: 'var(--font-mono)', fontWeight: 600 }}>
                    {hovered.countryState.getTotalMilitaryUnits().toLocaleString()}
                  </span>
                </div>
              )}
            </div>
          )}

          <div style={{ marginTop: '8px', fontSize: '10px', color: 'rgba(255,255,255,0.4)', textAlign: 'center' }}>
            Click to inspect sovereign state
          </div>
        </div>
      )}
    </div>
  );
};
