import React, { useState } from 'react';
import { GameSession } from '../../domain/models/GameSession';
import { SimulationEngine } from '../../domain/services/SimulationEngine';
import { TaxType } from '../../domain/models/types';
import {
  AdjustTaxRateCommand,
  TakeLoanCommand,
  RepayLoanCommand,
  ConstructFacilityCommand,
  EnactEconomicPolicyCommand,
  RevokeEconomicPolicyCommand,
  ExecuteMonthlyLedgerCommand,
} from '../../domain/commands/EconomyCommands';
import {
  Coins,
  Building2,
  FileText,
  Landmark,
  X,
  Plus,
  Percent,
  Check,
  AlertCircle,
} from 'lucide-react';

interface EconomyPanelProps {
  session: GameSession;
  engine: SimulationEngine;
  onClose: () => void;
  onUpdate: () => void;
}

export const EconomyPanel: React.FC<EconomyPanelProps> = ({
  session,
  engine,
  onClose,
  onUpdate,
}) => {
  const [activeTab, setActiveTab] = useState<'taxes' | 'debt' | 'facilities' | 'policies' | 'ledger'>('taxes');
  const [feedback, setFeedback] = useState<{ msg: string; isError: boolean } | null>(null);

  const country = session.getPlayerCountry();
  if (!country) return null;

  const showMsg = (msg: string, isError: boolean = false) => {
    setFeedback({ msg, isError });
    setTimeout(() => setFeedback(null), 3500);
  };

  const handleAdjustTax = (type: TaxType, newRate: number) => {
    const cmd = new AdjustTaxRateCommand('IND', type, newRate);
    const res = engine.dispatch(cmd);
    if (res.isSuccess) {
      showMsg(`Adjusted ${type} to ${(newRate * 100).toFixed(0)}%`);
      onUpdate();
    } else {
      showMsg(res.errorMessage || 'Failed to adjust tax rate', true);
    }
  };

  const handleTakeLoan = (amount: number) => {
    const cmd = new TakeLoanCommand('IND', amount);
    const res = engine.dispatch(cmd);
    if (res.isSuccess) {
      showMsg(`Secured loan of $${(amount / 1_000_000_000).toFixed(1)}B`);
      onUpdate();
    } else {
      showMsg(res.errorMessage || 'Loan rejected', true);
    }
  };

  const handleRepayLoan = (amount: number) => {
    const cmd = new RepayLoanCommand('IND', amount);
    const res = engine.dispatch(cmd);
    if (res.isSuccess) {
      showMsg(`Repaid $${(amount / 1_000_000_000).toFixed(1)}B towards sovereign debt`);
      onUpdate();
    } else {
      showMsg(res.errorMessage || 'Repayment failed', true);
    }
  };

  const handleConstructFacility = (facilityType: string, qty: number) => {
    const cmd = new ConstructFacilityCommand('IND', facilityType, qty);
    const res = engine.dispatch(cmd);
    if (res.isSuccess) {
      showMsg(`Commissioned ${qty}x ${facilityType}`);
      onUpdate();
    } else {
      showMsg(res.errorMessage || 'Construction failed', true);
    }
  };

  const handleTogglePolicy = (policyId: string, isActive: boolean) => {
    const cmd = isActive
      ? new RevokeEconomicPolicyCommand('IND', policyId)
      : new EnactEconomicPolicyCommand('IND', policyId);
    const res = engine.dispatch(cmd);
    if (res.isSuccess) {
      showMsg(isActive ? `Revoked policy '${policyId}'` : `Enacted policy '${policyId}'`);
      onUpdate();
    } else {
      showMsg(res.errorMessage || 'Policy change failed', true);
    }
  };

  const handleExecuteLedger = () => {
    const cmd = new ExecuteMonthlyLedgerCommand('IND');
    const res = engine.dispatch(cmd);
    if (res.isSuccess) {
      showMsg('Monthly accounting ledger executed and balanced');
      onUpdate();
    } else {
      showMsg(res.errorMessage || 'Ledger execution failed', true);
    }
  };

  const facilityDefs = [
    { type: 'Farms', name: 'Agricultural Farms', cost: 100_000_000, ironCost: 500, desc: 'Produces +15,000 Food/mo' },
    { type: 'PowerPlants', name: 'Power Plants', cost: 300_000_000, ironCost: 2000, desc: 'Generates +20,000 Energy/mo' },
    { type: 'Mines', name: 'Iron & Mineral Mines', cost: 250_000_000, ironCost: 200, desc: 'Extracts +8,000 Iron/mo' },
    { type: 'OilWells', name: 'Crude Oil Refineries', cost: 350_000_000, ironCost: 1500, desc: 'Extracts +10,000 Oil/mo' },
    { type: 'Factories', name: 'Heavy Industrial Plants', cost: 200_000_000, ironCost: 1000, desc: 'Produces +12,000 Consumer Goods/mo' },
    { type: 'Hospitals', name: 'Medical Centers', cost: 200_000_000, ironCost: 500, desc: 'Boosts Quality of Living' },
    { type: 'Schools', name: 'Schools & Universities', cost: 150_000_000, ironCost: 300, desc: 'Boosts Education & Living Standards' },
    { type: 'Barracks', name: 'Military Barracks', cost: 150_000_000, ironCost: 800, desc: 'Increases Armed Forces Capacity' },
    { type: 'UraniumMines', name: 'Uranium Extraction Sites', cost: 600_000_000, ironCost: 3000, desc: 'Extracts Strategic Nuclear Material' },
  ];

  const defaultPolicies = [
    { id: 'agricultural_subsidies', name: 'Agricultural Subsidies', cost: 500_000_000, upkeep: 50_000_000, desc: 'State subsidies for farmers. Increases Food yield by 20% and public approval by +3%.' },
    { id: 'industrial_modernization', name: 'Industrial Modernization', cost: 1_200_000_000, upkeep: 100_000_000, desc: 'Automation incentives and high-tech manufacturing grants. Boosts industrial GDP output by 15%.' },
    { id: 'infrastructure_stimulus', name: 'National Logistics Corridor', cost: 2_000_000_000, upkeep: 150_000_000, desc: 'High-speed freight corridors and expressway networks. Reduces trade and transport costs.' },
    { id: 'green_energy_transition', name: 'Renewable Clean Energy Transition', cost: 1_500_000_000, upkeep: 80_000_000, desc: 'Solar and wind subsidies. Reduces environmental degradation and increases Energy reserves.' },
    { id: 'foreign_investment_zones', name: 'Special Economic Zones (SEZ)', cost: 800_000_000, upkeep: 40_000_000, desc: 'Tax incentives for multinational corporate manufacturing. Increases Foreign Direct Investment.' },
    { id: 'universal_digital_id', name: 'Universal Digital Public Stack', cost: 600_000_000, upkeep: 30_000_000, desc: 'Direct benefit transfers and digitized financial rails. Decreases Corruption by -6.0%.' },
  ];

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
          <Coins size={22} color="var(--accent-gold)" />
          <div>
            <div style={{ fontSize: '18px', fontWeight: 700 }}>Ministry of Finance & National Economy</div>
            <div style={{ fontSize: '12px', color: 'var(--text-muted)' }}>Treasury, Fiscal Taxation, Sovereign Debt, and Industrial Production</div>
          </div>
        </div>

        <button className="btn" onClick={onClose} style={{ padding: '6px' }}>
          <X size={18} />
        </button>
      </div>

      {/* Tabs */}
      <div style={{ display: 'flex', gap: '8px', padding: '10px 20px', borderBottom: '1px solid var(--border-subtle)', background: 'rgba(0,0,0,0.2)' }}>
        <button className={`btn ${activeTab === 'taxes' ? 'btn-active' : ''}`} onClick={() => setActiveTab('taxes')}>
          <Percent size={14} /> Tax Sliders
        </button>
        <button className={`btn ${activeTab === 'debt' ? 'btn-active' : ''}`} onClick={() => setActiveTab('debt')}>
          <Landmark size={14} /> Central Bank & Debt
        </button>
        <button className={`btn ${activeTab === 'facilities' ? 'btn-active' : ''}`} onClick={() => setActiveTab('facilities')}>
          <Building2 size={14} /> Industrial Facilities
        </button>
        <button className={`btn ${activeTab === 'policies' ? 'btn-active' : ''}`} onClick={() => setActiveTab('policies')}>
          <FileText size={14} /> Economic Policies
        </button>
        <button className={`btn ${activeTab === 'ledger' ? 'btn-active' : ''}`} onClick={() => setActiveTab('ledger')}>
          <Coins size={14} /> Monthly Ledger
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
        {/* TAB 1: TAX SLIDERS */}
        {activeTab === 'taxes' && (
          <div style={{ display: 'flex', flexDirection: 'column', gap: '20px' }}>
            <div style={{ fontSize: '13px', color: 'var(--text-muted)' }}>
              Adjust national tax rates. Higher taxes increase state revenue but depress public approval rating and economic growth.
            </div>

            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(280px, 1fr))', gap: '16px' }}>
              {/* Income Tax */}
              <div className="glass-panel" style={{ padding: '16px' }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '8px' }}>
                  <span style={{ fontWeight: 600 }}>Personal Income Tax</span>
                  <span style={{ fontFamily: 'var(--font-mono)', fontWeight: 700, color: 'var(--accent-gold)' }}>
                    {(country.incomeTaxRate * 100).toFixed(0)}%
                  </span>
                </div>
                <input
                  type="range"
                  min="0"
                  max="0.50"
                  step="0.01"
                  value={country.incomeTaxRate}
                  onChange={(e) => handleAdjustTax(TaxType.IncomeTax, parseFloat(e.target.value))}
                  style={{ width: '100%', accentColor: 'var(--accent-gold)' }}
                />
                <div style={{ fontSize: '11px', color: 'var(--text-muted)', marginTop: '8px' }}>
                  Levied on wages and household income across 1.4B citizens.
                </div>
              </div>

              {/* Corporate Tax */}
              <div className="glass-panel" style={{ padding: '16px' }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '8px' }}>
                  <span style={{ fontWeight: 600 }}>Corporate Profit Tax</span>
                  <span style={{ fontFamily: 'var(--font-mono)', fontWeight: 700, color: 'var(--accent-gold)' }}>
                    {(country.corporateTaxRate * 100).toFixed(0)}%
                  </span>
                </div>
                <input
                  type="range"
                  min="0"
                  max="0.50"
                  step="0.01"
                  value={country.corporateTaxRate}
                  onChange={(e) => handleAdjustTax(TaxType.CorporateTax, parseFloat(e.target.value))}
                  style={{ width: '100%', accentColor: 'var(--accent-gold)' }}
                />
                <div style={{ fontSize: '11px', color: 'var(--text-muted)', marginTop: '8px' }}>
                  Taxation on domestic enterprises and multinational conglomerates.
                </div>
              </div>

              {/* Value Added Tax (VAT / GST) */}
              <div className="glass-panel" style={{ padding: '16px' }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '8px' }}>
                  <span style={{ fontWeight: 600 }}>Goods & Services Tax (GST / VAT)</span>
                  <span style={{ fontFamily: 'var(--font-mono)', fontWeight: 700, color: 'var(--accent-gold)' }}>
                    {(country.vatRate * 100).toFixed(0)}%
                  </span>
                </div>
                <input
                  type="range"
                  min="0"
                  max="0.30"
                  step="0.01"
                  value={country.vatRate}
                  onChange={(e) => handleAdjustTax(TaxType.Vat, parseFloat(e.target.value))}
                  style={{ width: '100%', accentColor: 'var(--accent-gold)' }}
                />
                <div style={{ fontSize: '11px', color: 'var(--text-muted)', marginTop: '8px' }}>
                  Consumption tax on domestic commerce and traded retail goods.
                </div>
              </div>
            </div>
          </div>
        )}

        {/* TAB 2: CENTRAL BANK & DEBT */}
        {activeTab === 'debt' && (
          <div style={{ display: 'flex', flexDirection: 'column', gap: '20px' }}>
            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: '14px' }}>
              <div className="glass-panel" style={{ padding: '16px' }}>
                <div style={{ color: 'var(--text-muted)', fontSize: '12px' }}>Current Outstanding Debt</div>
                <div style={{ fontSize: '22px', fontWeight: 700, color: country.debt > 0 ? '#f87171' : '#34d399', fontFamily: 'var(--font-mono)' }}>
                  ${(country.debt / 1_000_000_000).toFixed(2)}B
                </div>
              </div>

              <div className="glass-panel" style={{ padding: '16px' }}>
                <div style={{ color: 'var(--text-muted)', fontSize: '12px' }}>Credit Rating</div>
                <div style={{ fontSize: '22px', fontWeight: 700, color: 'var(--accent-gold)' }}>
                  {country.creditRating}
                </div>
                <div style={{ fontSize: '11px', color: 'var(--text-muted)' }}>
                  Monthly Interest: {(country.getMonthlyInterestRate() * 100).toFixed(2)}%
                </div>
              </div>

              <div className="glass-panel" style={{ padding: '16px' }}>
                <div style={{ color: 'var(--text-muted)', fontSize: '12px' }}>Liquid Treasury</div>
                <div style={{ fontSize: '22px', fontWeight: 700, color: '#34d399', fontFamily: 'var(--font-mono)' }}>
                  ${(country.treasury / 1_000_000_000).toFixed(2)}B
                </div>
              </div>
            </div>

            {/* Borrow & Repay Actions */}
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px' }}>
              <div className="glass-panel" style={{ padding: '16px' }}>
                <div style={{ fontWeight: 600, marginBottom: '8px' }}>Take Sovereign Loan (IMF / Central Bank)</div>
                <div style={{ fontSize: '12px', color: 'var(--text-muted)', marginBottom: '12px' }}>
                  Injects cash into treasury immediately. Excessive debt degrades credit ratings.
                </div>
                <div style={{ display: 'flex', flexWrap: 'wrap', gap: '8px' }}>
                  <button className="btn" onClick={() => handleTakeLoan(1_000_000_000)}>+$1 Billion</button>
                  <button className="btn" onClick={() => handleTakeLoan(5_000_000_000)}>+$5 Billion</button>
                  <button className="btn" onClick={() => handleTakeLoan(10_000_000_000)}>+$10 Billion</button>
                  <button className="btn" onClick={() => handleTakeLoan(25_000_000_000)}>+$25 Billion</button>
                </div>
              </div>

              <div className="glass-panel" style={{ padding: '16px' }}>
                <div style={{ fontWeight: 600, marginBottom: '8px' }}>Repay Sovereign Debt</div>
                <div style={{ fontSize: '12px', color: 'var(--text-muted)', marginBottom: '12px' }}>
                  Pay down national liabilities to restore AAA credit rating and eliminate interest drag.
                </div>
                <div style={{ display: 'flex', flexWrap: 'wrap', gap: '8px' }}>
                  <button className="btn btn-success" onClick={() => handleRepayLoan(1_000_000_000)} disabled={country.debt <= 0}>
                    -$1 Billion
                  </button>
                  <button className="btn btn-success" onClick={() => handleRepayLoan(5_000_000_000)} disabled={country.debt <= 0}>
                    -$5 Billion
                  </button>
                  <button className="btn btn-success" onClick={() => handleRepayLoan(10_000_000_000)} disabled={country.debt <= 0}>
                    -$10 Billion
                  </button>
                  <button className="btn btn-success" onClick={() => handleRepayLoan(country.debt)} disabled={country.debt <= 0}>
                    Pay Off Entire Debt
                  </button>
                </div>
              </div>
            </div>
          </div>
        )}

        {/* TAB 3: FACILITIES */}
        {activeTab === 'facilities' && (
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(280px, 1fr))', gap: '14px' }}>
            {facilityDefs.map((f) => {
              const currentCount = country.facilities[f.type] ?? 0;
              return (
                <div key={f.type} className="glass-panel" style={{ padding: '14px', display: 'flex', flexDirection: 'column', justifyContent: 'space-between' }}>
                  <div>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                      <span style={{ fontWeight: 600, fontSize: '14px' }}>{f.name}</span>
                      <span className="badge badge-gold" style={{ fontFamily: 'var(--font-mono)' }}>{currentCount}</span>
                    </div>
                    <div style={{ fontSize: '11px', color: 'var(--text-muted)', margin: '4px 0 8px' }}>{f.desc}</div>
                    <div style={{ fontSize: '11px', color: 'var(--text-dim)', fontFamily: 'var(--font-mono)' }}>
                      Cost: ${(f.cost / 1_000_000).toFixed(0)}M • {f.ironCost} Iron
                    </div>
                  </div>

                  <div style={{ display: 'flex', gap: '6px', marginTop: '12px' }}>
                    <button className="btn btn-primary" style={{ flex: 1, padding: '6px' }} onClick={() => handleConstructFacility(f.type, 1)}>
                      <Plus size={14} /> Build 1
                    </button>
                    <button className="btn" style={{ padding: '6px 12px' }} onClick={() => handleConstructFacility(f.type, 5)}>
                      +5
                    </button>
                  </div>
                </div>
              );
            })}
          </div>
        )}

        {/* TAB 4: POLICIES */}
        {activeTab === 'policies' && (
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(300px, 1fr))', gap: '14px' }}>
            {defaultPolicies.map((p) => {
              const isActive = country.activePolicies.includes(p.id);
              return (
                <div
                  key={p.id}
                  className="glass-panel"
                  style={{
                    padding: '16px',
                    borderColor: isActive ? 'var(--accent-gold)' : 'var(--border-subtle)',
                    display: 'flex',
                    flexDirection: 'column',
                    justifyContent: 'space-between',
                  }}
                >
                  <div>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                      <span style={{ fontWeight: 600, fontSize: '15px' }}>{p.name}</span>
                      {isActive && <span className="badge badge-gold">Active</span>}
                    </div>
                    <div style={{ fontSize: '12px', color: 'var(--text-muted)', margin: '8px 0' }}>{p.desc}</div>
                    <div style={{ fontSize: '11px', color: 'var(--text-dim)', fontFamily: 'var(--font-mono)' }}>
                      Enact Fee: ${(p.cost / 1_000_000).toFixed(0)}M • Upkeep: ${(p.upkeep / 1_000_000).toFixed(0)}M/mo
                    </div>
                  </div>

                  <button
                    className={`btn ${isActive ? 'btn-danger' : 'btn-primary'}`}
                    style={{ marginTop: '12px' }}
                    onClick={() => handleTogglePolicy(p.id, isActive)}
                  >
                    {isActive ? 'Revoke Policy' : 'Enact Policy'}
                  </button>
                </div>
              );
            })}
          </div>
        )}

        {/* TAB 5: MONTHLY LEDGER */}
        {activeTab === 'ledger' && (
          <div style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <div style={{ fontSize: '13px', color: 'var(--text-muted)' }}>
                Comprehensive monthly financial reconciliation ledger and accounting balance.
              </div>
              <button className="btn btn-primary" onClick={handleExecuteLedger}>
                Execute Monthly Accounting Now
              </button>
            </div>

            {country.lastBudgetReport ? (
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px' }}>
                {/* Revenues */}
                <div className="glass-panel" style={{ padding: '16px' }}>
                  <div style={{ fontWeight: 700, color: '#34d399', marginBottom: '12px', fontSize: '14px' }}>
                    Gross Monthly Revenue: ${(country.lastBudgetReport.grossRevenue / 1_000_000).toFixed(1)}M
                  </div>
                  <div style={{ display: 'flex', flexDirection: 'column', gap: '8px', fontSize: '12px', fontFamily: 'var(--font-mono)' }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                      <span>Income Tax:</span>
                      <span>+${(country.lastBudgetReport.incomeTaxRevenue / 1_000_000).toFixed(1)}M</span>
                    </div>
                    <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                      <span>Corporate Tax:</span>
                      <span>+${(country.lastBudgetReport.corporateTaxRevenue / 1_000_000).toFixed(1)}M</span>
                    </div>
                    <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                      <span>Goods & Services (VAT):</span>
                      <span>+${(country.lastBudgetReport.vatRevenue / 1_000_000).toFixed(1)}M</span>
                    </div>
                    <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                      <span>Trade Exports:</span>
                      <span>+${(country.lastBudgetReport.tradeExportRevenue / 1_000_000).toFixed(1)}M</span>
                    </div>
                  </div>
                </div>

                {/* Expenses */}
                <div className="glass-panel" style={{ padding: '16px' }}>
                  <div style={{ fontWeight: 700, color: '#f87171', marginBottom: '12px', fontSize: '14px' }}>
                    Gross Monthly Expenses: ${(country.lastBudgetReport.grossExpenses / 1_000_000).toFixed(1)}M
                  </div>
                  <div style={{ display: 'flex', flexDirection: 'column', gap: '8px', fontSize: '12px', fontFamily: 'var(--font-mono)' }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                      <span>Civil Administration:</span>
                      <span>-${(country.lastBudgetReport.civilAdministrationExpenses / 1_000_000).toFixed(1)}M</span>
                    </div>
                    <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                      <span>Ministry Operations:</span>
                      <span>-${(country.lastBudgetReport.ministryOperationalExpenses / 1_000_000).toFixed(1)}M</span>
                    </div>
                    <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                      <span>Military Maintenance:</span>
                      <span>-${(country.lastBudgetReport.militaryUnitUpkeepExpenses / 1_000_000).toFixed(1)}M</span>
                    </div>
                    <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                      <span>Debt Service (Interest):</span>
                      <span>-${(country.lastBudgetReport.debtInterestExpenses / 1_000_000).toFixed(1)}M</span>
                    </div>
                  </div>
                </div>
              </div>
            ) : (
              <div style={{ textAlign: 'center', padding: '40px', color: 'var(--text-muted)' }}>
                No budget cycle has completed yet. Ledger will populate automatically at month boundary.
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  );
};
