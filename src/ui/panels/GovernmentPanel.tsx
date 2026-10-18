import React, { useState } from 'react';
import { GameSession } from '../../domain/models/GameSession';
import { SimulationEngine } from '../../domain/services/SimulationEngine';
import { MinistryType, FundingLevel, IdeologyType } from '../../domain/models/types';
import { MinisterRosterService, MinisterCandidate } from '../../domain/services/MinisterRosterService';
import { IDEOLOGY_DEFINITIONS } from '../../domain/models/Ideology';
import {
  AppointMinisterCommand,
  DismissMinisterCommand,
  AdjustMinistryFundingCommand,
  ChangeIdeologyCommand,
  FundPresidentialProtectionCommand,
  LaunchAntiCorruptionCampaignCommand,
  LaunchCounterTerrorismCommand,
} from '../../domain/commands/GovernmentCommands';
import {
  Landmark,
  UserCheck,
  UserX,
  Shield,
  X,
  Check,
  AlertCircle,
  Scale,
} from 'lucide-react';

interface GovernmentPanelProps {
  session: GameSession;
  engine: SimulationEngine;
  onClose: () => void;
  onUpdate: () => void;
}

export const GovernmentPanel: React.FC<GovernmentPanelProps> = ({
  session,
  engine,
  onClose,
  onUpdate,
}) => {
  const [activeTab, setActiveTab] = useState<'cabinet' | 'ideology' | 'security'>('cabinet');
  const [selectedMinistryForHire, setSelectedMinistryForHire] = useState<MinistryType | null>(null);
  const [feedback, setFeedback] = useState<{ msg: string; isError: boolean } | null>(null);

  const country = session.getPlayerCountry();
  if (!country) return null;

  const showMsg = (msg: string, isError: boolean = false) => {
    setFeedback({ msg, isError });
    setTimeout(() => setFeedback(null), 3500);
  };

  const handleSetFunding = (ministryType: MinistryType, funding: FundingLevel) => {
    const cmd = new AdjustMinistryFundingCommand('IND', ministryType, funding);
    const res = engine.dispatch(cmd);
    if (res.isSuccess) {
      showMsg(`Updated ${ministryType} funding to ${FundingLevel[funding]}`);
      onUpdate();
    } else {
      showMsg(res.errorMessage || 'Failed to update funding', true);
    }
  };

  const handleDismissMinister = (ministryType: MinistryType) => {
    const cmd = new DismissMinisterCommand('IND', ministryType);
    const res = engine.dispatch(cmd);
    if (res.isSuccess) {
      showMsg(`Dismissed Minister of ${ministryType}`);
      onUpdate();
    } else {
      showMsg(res.errorMessage || 'Failed to dismiss minister', true);
    }
  };

  const handleAppointMinister = (ministryType: MinistryType, candidate: MinisterCandidate) => {
    const cmd = new AppointMinisterCommand('IND', ministryType, candidate, candidate.hiringFee);
    const res = engine.dispatch(cmd);
    if (res.isSuccess) {
      showMsg(`Appointed ${candidate.name} as Minister of ${ministryType}`);
      setSelectedMinistryForHire(null);
      onUpdate();
    } else {
      showMsg(res.errorMessage || 'Failed to appoint minister', true);
    }
  };

  const handleChangeIdeology = (newIdeology: IdeologyType) => {
    const cmd = new ChangeIdeologyCommand('IND', newIdeology);
    const res = engine.dispatch(cmd);
    if (res.isSuccess) {
      showMsg(`Reformed state governance to ${IDEOLOGY_DEFINITIONS[newIdeology].displayName}`);
      onUpdate();
    } else {
      showMsg(res.errorMessage || 'Ideology reform failed', true);
    }
  };

  const handleSetProtection = (level: FundingLevel) => {
    const cmd = new FundPresidentialProtectionCommand('IND', level);
    const res = engine.dispatch(cmd);
    if (res.isSuccess) {
      showMsg(`Presidential security set to ${FundingLevel[level]}`);
      onUpdate();
    } else {
      showMsg(res.errorMessage || 'Failed to update protection', true);
    }
  };

  const handleAntiCorruption = () => {
    const cmd = new LaunchAntiCorruptionCampaignCommand('IND', 500_000_000);
    const res = engine.dispatch(cmd);
    if (res.isSuccess) {
      showMsg('Launched federal Anti-Corruption crackdown. Corruption reduced by -6%');
      onUpdate();
    } else {
      showMsg(res.errorMessage || 'Anti-corruption failed', true);
    }
  };

  const handleCounterTerrorism = () => {
    const cmd = new LaunchCounterTerrorismCommand('IND');
    const res = engine.dispatch(cmd);
    if (res.isSuccess) {
      showMsg('Dispatched federal counter-terrorism operations. National stability bolstered');
      onUpdate();
    } else {
      showMsg(res.errorMessage || 'Counter-terrorism operation failed', true);
    }
  };

  return (
    <div
      style={{
        position: 'fixed',
        top: '60px',
        left: '50%',
        transform: 'translateX(-50%)',
        width: '94%',
        maxWidth: '1000px',
        maxHeight: 'calc(100vh - 140px)',
        zIndex: 100,
        display: 'flex',
        flexDirection: 'column',
      }}
      className="glass-modal animate-fade-in"
    >
      {/* Header */}
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', padding: '16px 20px', borderBottom: '1px solid var(--border-subtle)' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
          <Landmark size={22} color="var(--accent-gold)" />
          <div>
            <div style={{ fontSize: '18px', fontWeight: 700 }}>Council of Ministers & State Governance</div>
            <div style={{ fontSize: '12px', color: 'var(--text-muted)' }}>Cabinet Appointments, Portfolio Funding, State Ideology, and Internal Security</div>
          </div>
        </div>

        <button className="btn" onClick={onClose} style={{ padding: '6px' }}>
          <X size={18} />
        </button>
      </div>

      {/* Tabs */}
      <div style={{ display: 'flex', gap: '8px', padding: '10px 20px', borderBottom: '1px solid var(--border-subtle)', background: 'rgba(0,0,0,0.2)' }}>
        <button className={`btn ${activeTab === 'cabinet' ? 'btn-active' : ''}`} onClick={() => setActiveTab('cabinet')}>
          <UserCheck size={14} /> Cabinet Ministers (13)
        </button>
        <button className={`btn ${activeTab === 'ideology' ? 'btn-active' : ''}`} onClick={() => setActiveTab('ideology')}>
          <Scale size={14} /> State Ideology
        </button>
        <button className={`btn ${activeTab === 'security' ? 'btn-active' : ''}`} onClick={() => setActiveTab('security')}>
          <Shield size={14} /> Security & Anti-Corruption
        </button>
      </div>

      {/* Feedback banner */}
      {feedback && (
        <div
          style={{
            margin: '8px 20px 0',
            padding: '8px 14px',
            borderRadius: 'var(--radius-md)',
            background: feedback.isError ? 'rgba(239, 68, 68, 0.2)' : 'rgba(16, 185, 129, 0.2)',
            border: feedback.isError ? '1px solid var(--accent-red)' : '1px solid var(--accent-emerald)',
            color: feedback.isError ? '#f87171' : '#34d399',
            fontSize: '13px',
            display: 'flex',
            alignItems: 'center',
            gap: '8px',
          }}
        >
          {feedback.isError ? <AlertCircle size={16} /> : <Check size={16} />}
          <span>{feedback.msg}</span>
        </div>
      )}

      {/* Content Area */}
      <div style={{ padding: '20px', overflowY: 'auto', flex: 1 }}>
        {/* TAB 1: CABINET MINISTERS */}
        {activeTab === 'cabinet' && (
          <div style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
            <div style={{ fontSize: '13px', color: 'var(--text-muted)' }}>
              Appoint seasoned ministers to boost portfolio efficiency. Higher funding levels provide resource multipliers. Zero funding causes civil unrest and strikes.
            </div>

            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(300px, 1fr))', gap: '14px' }}>
              {Object.values(MinistryType).map((mType) => {
                const ministry = country.ministries[mType];
                const efficiency = ministry?.calculateEfficiency ? ministry.calculateEfficiency() : 0.70;

                return (
                  <div key={mType} className="glass-panel" style={{ padding: '14px', display: 'flex', flexDirection: 'column', gap: '10px' }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                      <span style={{ fontWeight: 600, fontSize: '14px' }}>{mType.replace(/([A-Z])/g, ' $1').trim()}</span>
                      <span
                        className="badge"
                        style={{
                          background: efficiency >= 0.8 ? 'rgba(16, 185, 129, 0.2)' : 'rgba(245, 158, 11, 0.2)',
                          color: efficiency >= 0.8 ? '#34d399' : '#fbbf24',
                        }}
                      >
                        {(efficiency * 100).toFixed(0)}% Eff
                      </span>
                    </div>

                    {/* Minister info */}
                    <div style={{ background: 'rgba(0,0,0,0.25)', padding: '8px 12px', borderRadius: 'var(--radius-sm)', fontSize: '12px' }}>
                      {ministry?.activeMinister ? (
                        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                          <div>
                            <div style={{ fontWeight: 600, color: '#fff' }}>{ministry.activeMinister.name}</div>
                            <div style={{ color: 'var(--text-dim)', fontSize: '11px' }}>
                              Competence: {(ministry.activeMinister.competence * 100).toFixed(0)}% • Salary: ${(ministry.activeMinister.monthlySalary / 1000).toFixed(0)}k/mo
                            </div>
                          </div>
                          <button className="btn btn-danger" style={{ padding: '4px 8px', fontSize: '11px' }} onClick={() => handleDismissMinister(mType)}>
                            <UserX size={12} /> Dismiss
                          </button>
                        </div>
                      ) : (
                        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                          <span style={{ color: 'var(--text-muted)' }}>Vacant (Civil Servant Baseline: 70%)</span>
                          <button className="btn btn-primary" style={{ padding: '4px 10px', fontSize: '11px' }} onClick={() => setSelectedMinistryForHire(mType)}>
                            <UserCheck size={12} /> Appoint
                          </button>
                        </div>
                      )}
                    </div>

                    {/* Funding Slider Buttons */}
                    <div>
                      <div style={{ fontSize: '11px', color: 'var(--text-muted)', marginBottom: '4px' }}>Funding Tier:</div>
                      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(5, 1fr)', gap: '4px' }}>
                        {[FundingLevel.Zero, FundingLevel.Low, FundingLevel.Medium, FundingLevel.High, FundingLevel.Maximum].map((lvl) => {
                          const isCurrent = (ministry?.funding ?? FundingLevel.Medium) === lvl;
                          return (
                            <button
                              key={lvl}
                              className={`btn ${isCurrent ? 'btn-active' : ''}`}
                              style={{ padding: '3px 0', fontSize: '10px', textAlign: 'center' }}
                              onClick={() => handleSetFunding(mType, lvl)}
                            >
                              {FundingLevel[lvl]}
                            </button>
                          );
                        })}
                      </div>
                    </div>
                  </div>
                );
              })}
            </div>
          </div>
        )}

        {/* TAB 2: STATE IDEOLOGY */}
        {activeTab === 'ideology' && (
          <div style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
            <div style={{ fontSize: '13px', color: 'var(--text-muted)' }}>
              Choose national constitutional governance model. Changing state ideology requires substantial political capital transition fees and imposes a 12-month cooldown.
            </div>

            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(280px, 1fr))', gap: '14px' }}>
              {Object.values(IDEOLOGY_DEFINITIONS).map((def) => {
                const isCurrent = country.ideology === def.type;
                const canAfford = country.treasury >= def.transitionCost;
                const onCooldown = country.ideologyCooldownMonths > 0;

                return (
                  <div
                    key={def.type}
                    className="glass-panel"
                    style={{
                      padding: '16px',
                      borderColor: isCurrent ? 'var(--accent-gold)' : 'var(--border-subtle)',
                      display: 'flex',
                      flexDirection: 'column',
                      justifyContent: 'space-between',
                    }}
                  >
                    <div>
                      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                        <span style={{ fontWeight: 700, fontSize: '15px' }}>{def.displayName}</span>
                        {isCurrent && <span className="badge badge-gold">Active</span>}
                      </div>

                      <div style={{ fontSize: '12px', color: 'var(--text-muted)', margin: '8px 0' }}>
                        {def.description}
                      </div>

                      <div style={{ display: 'flex', flexDirection: 'column', gap: '4px', fontSize: '11px', fontFamily: 'var(--font-mono)' }}>
                        <div>Tax Efficiency: {(def.taxEfficiencyMultiplier * 100).toFixed(0)}%</div>
                        <div>Stability Modifier: {def.stabilityModifier >= 0 ? `+${def.stabilityModifier}%` : `${def.stabilityModifier}%`}</div>
                        <div>Approval Modifier: {def.approvalModifier >= 0 ? `+${def.approvalModifier}%` : `${def.approvalModifier}%`}</div>
                        <div style={{ color: 'var(--text-dim)', marginTop: '4px' }}>
                          Transition Fee: ${(def.transitionCost / 1_000_000_000).toFixed(1)}B
                        </div>
                      </div>
                    </div>

                    {!isCurrent && (
                      <button
                        className="btn btn-primary"
                        style={{ marginTop: '12px' }}
                        disabled={!canAfford || onCooldown}
                        onClick={() => handleChangeIdeology(def.type)}
                      >
                        {onCooldown ? `Cooldown (${country.ideologyCooldownMonths}m)` : `Reform to ${def.displayName}`}
                      </button>
                    )}
                  </div>
                );
              })}
            </div>
          </div>
        )}

        {/* TAB 3: SECURITY & CRIME */}
        {activeTab === 'security' && (
          <div style={{ display: 'flex', flexDirection: 'column', gap: '20px' }}>
            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(280px, 1fr))', gap: '16px' }}>
              {/* Presidential Protection */}
              <div className="glass-panel" style={{ padding: '16px' }}>
                <div style={{ fontWeight: 600, fontSize: '15px', marginBottom: '4px' }}>Executive Protection Level</div>
                <div style={{ fontSize: '12px', color: 'var(--text-muted)', marginBottom: '12px' }}>
                  Special Protection Group funding tier protecting state executive against assassination and hostile coups.
                </div>
                <div style={{ display: 'grid', gridTemplateColumns: 'repeat(5, 1fr)', gap: '4px' }}>
                  {[FundingLevel.Zero, FundingLevel.Low, FundingLevel.Medium, FundingLevel.High, FundingLevel.Maximum].map((lvl) => {
                    const isCurrent = country.presidentialProtectionLevel === lvl;
                    return (
                      <button
                        key={lvl}
                        className={`btn ${isCurrent ? 'btn-active' : ''}`}
                        style={{ padding: '6px 0', fontSize: '11px' }}
                        onClick={() => handleSetProtection(lvl)}
                      >
                        {FundingLevel[lvl]}
                      </button>
                    );
                  })}
                </div>
              </div>

              {/* Anti-Corruption */}
              <div className="glass-panel" style={{ padding: '16px' }}>
                <div style={{ fontWeight: 600, fontSize: '15px', marginBottom: '4px' }}>Federal Anti-Corruption Campaign</div>
                <div style={{ fontSize: '12px', color: 'var(--text-muted)', marginBottom: '8px' }}>
                  Current Corruption Index: <strong style={{ color: country.corruptionIndex > 30 ? '#f87171' : '#34d399' }}>{country.corruptionIndex.toFixed(1)}/100</strong>
                </div>
                <button className="btn btn-primary" style={{ width: '100%', marginTop: '8px' }} onClick={handleAntiCorruption}>
                  Launch Anti-Corruption Crackdown ($500M)
                </button>
              </div>

              {/* Counter-Terrorism */}
              <div className="glass-panel" style={{ padding: '16px' }}>
                <div style={{ fontWeight: 600, fontSize: '15px', marginBottom: '4px' }}>National Counter-Terrorism Grid</div>
                <div style={{ fontSize: '12px', color: 'var(--text-muted)', marginBottom: '8px' }}>
                  Deploy National Security Guard and intelligence networks to neutralize domestic terror cells.
                </div>
                <button className="btn btn-primary" style={{ width: '100%', marginTop: '8px' }} onClick={handleCounterTerrorism}>
                  Authorize Counter-Terrorism Sweep ($250M)
                </button>
              </div>
            </div>
          </div>
        )}
      </div>

      {/* Candidate Selection Sub-Modal */}
      {selectedMinistryForHire && (
        <div
          style={{
            position: 'absolute',
            top: 0,
            left: 0,
            right: 0,
            bottom: 0,
            background: 'rgba(7, 10, 18, 0.95)',
            zIndex: 110,
            padding: '24px',
            display: 'flex',
            flexDirection: 'column',
          }}
          className="animate-fade-in"
        >
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '16px' }}>
            <div>
              <div style={{ fontSize: '18px', fontWeight: 700 }}>
                Appoint Minister of {selectedMinistryForHire}
              </div>
              <div style={{ fontSize: '12px', color: 'var(--text-muted)' }}>
                Select an expert candidate to assume administrative leadership
              </div>
            </div>
            <button className="btn" onClick={() => setSelectedMinistryForHire(null)}>
              <X size={18} />
            </button>
          </div>

          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(280px, 1fr))', gap: '14px', overflowY: 'auto' }}>
            {MinisterRosterService.getCandidates(selectedMinistryForHire).map((cand) => (
              <div key={cand.id} className="glass-panel" style={{ padding: '16px', display: 'flex', flexDirection: 'column', justifyContent: 'space-between' }}>
                <div>
                  <div style={{ fontWeight: 700, fontSize: '15px' }}>{cand.name}</div>
                  <div style={{ fontSize: '12px', color: 'var(--accent-gold)', margin: '4px 0' }}>
                    Competence: {(cand.competence * 100).toFixed(0)}%
                  </div>
                  <div style={{ fontSize: '12px', color: 'var(--text-muted)', marginBottom: '10px' }}>
                    {cand.bio}
                  </div>
                  <div style={{ fontSize: '11px', color: 'var(--text-dim)', fontFamily: 'var(--font-mono)' }}>
                    Hiring Fee: ${(cand.hiringFee / 1_000_000).toFixed(0)}M • Monthly Salary: ${(cand.monthlySalary / 1000).toFixed(0)}k/mo
                  </div>
                </div>

                <button
                  className="btn btn-primary"
                  style={{ marginTop: '12px' }}
                  onClick={() => handleAppointMinister(selectedMinistryForHire, cand)}
                  disabled={country.treasury < cand.hiringFee}
                >
                  Appoint for ${(cand.hiringFee / 1_000_000).toFixed(0)}M
                </button>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
};
