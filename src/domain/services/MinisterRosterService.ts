import { MinistryType } from '../models/types';
import { Minister } from '../models/MinistryState';

export interface MinisterCandidate extends Minister {
  ministry: MinistryType;
  hiringFee: number;
  bio: string;
}

export class MinisterRosterService {
  private static readonly CANDIDATES_BY_MINISTRY: Record<string, MinisterCandidate[]> = {
    [MinistryType.Health]: [
      { id: 'CAND_HLTH_01', name: 'Dr. Soumya Swaminathan', ministry: MinistryType.Health, competence: 0.92, hiringFee: 20_000_000, monthlySalary: 350_000, bio: 'Renowned clinical epidemiologist with vast global health administrative experience.' },
      { id: 'CAND_HLTH_02', name: 'Dr. Rajesh Kotecha', ministry: MinistryType.Health, competence: 0.78, hiringFee: 12_000_000, monthlySalary: 220_000, bio: 'Public healthcare systems reformer and traditional medicine integration specialist.' },
      { id: 'CAND_HLTH_03', name: 'Dr. Randeep Guleria', ministry: MinistryType.Health, competence: 0.88, hiringFee: 18_000_000, monthlySalary: 300_000, bio: 'Distinguished pulmonologist and former hospital director.' }
    ],
    [MinistryType.Education]: [
      { id: 'CAND_EDU_01', name: 'Prof. K. Kasturirangan', ministry: MinistryType.Education, competence: 0.94, hiringFee: 25_000_000, monthlySalary: 400_000, bio: 'Architect of national education curriculum reforms and academic research vision.' },
      { id: 'CAND_EDU_02', name: 'Prof. Anil Sahasrabudhe', ministry: MinistryType.Education, competence: 0.82, hiringFee: 15_000_000, monthlySalary: 250_000, bio: 'Technical education reformer and engineering accreditation leader.' },
      { id: 'CAND_EDU_03', name: 'Dr. Manjul Bhargava', ministry: MinistryType.Education, competence: 0.86, hiringFee: 18_000_000, monthlySalary: 280_000, bio: 'Mathematician promoting foundational numeracy and science education.' }
    ],
    [MinistryType.Infrastructure]: [
      { id: 'CAND_INFRA_01', name: 'E. Sreedharan', ministry: MinistryType.Infrastructure, competence: 0.96, hiringFee: 30_000_000, monthlySalary: 500_000, bio: 'Celebrated master engineer known for punctuality, rapid transit, and mega-projects.' },
      { id: 'CAND_INFRA_02', name: 'Vinayak Chatterjee', ministry: MinistryType.Infrastructure, competence: 0.80, hiringFee: 14_000_000, monthlySalary: 240_000, bio: 'Infrastructure finance advisor and public-private partnership strategist.' },
      { id: 'CAND_INFRA_03', name: 'Sudhir Rao', ministry: MinistryType.Infrastructure, competence: 0.75, hiringFee: 10_000_000, monthlySalary: 180_000, bio: 'Highway network logistics and freight corridor project manager.' }
    ],
    [MinistryType.Defense]: [
      { id: 'CAND_DEF_01', name: 'Gen. Manoj Pande', ministry: MinistryType.Defense, competence: 0.92, hiringFee: 25_000_000, monthlySalary: 450_000, bio: 'Strategic combined arms commander focused on rapid force modernization.' },
      { id: 'CAND_DEF_02', name: 'Adm. R. Hari Kumar', ministry: MinistryType.Defense, competence: 0.85, hiringFee: 18_000_000, monthlySalary: 320_000, bio: 'Maritime security expert emphasizing blue-water presence and indigenization.' },
      { id: 'CAND_DEF_03', name: 'Air Chief Marshal V.R. Chaudhari', ministry: MinistryType.Defense, competence: 0.88, hiringFee: 20_000_000, monthlySalary: 360_000, bio: 'Aerospace doctrine specialist advocating indigenous fighter procurement.' }
    ],
    [MinistryType.Finance]: [
      { id: 'CAND_FIN_01', name: 'Dr. Raghuram Rajan', ministry: MinistryType.Finance, competence: 0.95, hiringFee: 30_000_000, monthlySalary: 500_000, bio: 'Acclaimed macroeconomic scholar with deep central banking and monetary authority.' },
      { id: 'CAND_FIN_02', name: 'Dr. Arvind Subramanian', ministry: MinistryType.Finance, competence: 0.86, hiringFee: 20_000_000, monthlySalary: 340_000, bio: 'Fiscal policy and international trade strategist.' },
      { id: 'CAND_FIN_03', name: 'Uday Kotak', ministry: MinistryType.Finance, competence: 0.84, hiringFee: 18_000_000, monthlySalary: 300_000, bio: 'Commercial banking veteran with sharp debt management and capital markets acumen.' }
    ],
    [MinistryType.InternalAffairs]: [
      { id: 'CAND_INT_01', name: 'Ajit Doval', ministry: MinistryType.InternalAffairs, competence: 0.95, hiringFee: 30_000_000, monthlySalary: 500_000, bio: 'Premier security strategist and intelligence architect.' },
      { id: 'CAND_INT_02', name: 'Praveen Sood', ministry: MinistryType.InternalAffairs, competence: 0.82, hiringFee: 15_000_000, monthlySalary: 260_000, bio: 'Law enforcement administrator focusing on police modernization and cyber forensics.' },
      { id: 'CAND_INT_03', name: 'Subodh Kumar Jaiswal', ministry: MinistryType.InternalAffairs, competence: 0.79, hiringFee: 12_000_000, monthlySalary: 200_000, bio: 'Senior investigative director specializing in federal anti-corruption and stability.' }
    ],
    [MinistryType.SecurityIntelligence]: [
      { id: 'CAND_SEC_01', name: 'Ravi Sinha', ministry: MinistryType.SecurityIntelligence, competence: 0.90, hiringFee: 25_000_000, monthlySalary: 400_000, bio: 'Foreign intelligence operative and cross-border threat analyst.' },
      { id: 'CAND_SEC_02', name: 'Tapan Deka', ministry: MinistryType.SecurityIntelligence, competence: 0.87, hiringFee: 20_000_000, monthlySalary: 320_000, bio: 'Counter-terrorism operations director with proven domestic infiltration track record.' },
      { id: 'CAND_SEC_03', name: 'S.M. Sahai', ministry: MinistryType.SecurityIntelligence, competence: 0.80, hiringFee: 14_000_000, monthlySalary: 230_000, bio: 'Strategic security analyst focused on border surveillance and cyber intelligence.' }
    ],
    [MinistryType.IndustryConstruction]: [
      { id: 'CAND_IND_01', name: 'N. Chandrasekaran', ministry: MinistryType.IndustryConstruction, competence: 0.93, hiringFee: 28_000_000, monthlySalary: 450_000, bio: 'Industrial conglomerate leader specializing in manufacturing supply chain scalability.' },
      { id: 'CAND_IND_02', name: 'Baba Kalyani', ministry: MinistryType.IndustryConstruction, competence: 0.85, hiringFee: 18_000_000, monthlySalary: 310_000, bio: 'Heavy forge industrialist and defense manufacturing champion.' },
      { id: 'CAND_IND_03', name: 'Anand Mahindra', ministry: MinistryType.IndustryConstruction, competence: 0.88, hiringFee: 22_000_000, monthlySalary: 380_000, bio: 'Automotive and aerospace industrialist fostering domestic manufacturing.' }
    ],
    [MinistryType.Environment]: [
      { id: 'CAND_ENV_01', name: 'Sunita Narain', ministry: MinistryType.Environment, competence: 0.91, hiringFee: 20_000_000, monthlySalary: 320_000, bio: 'Leading environmental activist, water conservationist and green policy advocate.' },
      { id: 'CAND_ENV_02', name: 'Dr. Vandana Shiva', ministry: MinistryType.Environment, competence: 0.80, hiringFee: 14_000_000, monthlySalary: 220_000, bio: 'Biodiversity protection and sustainable agriculture campaigner.' },
      { id: 'CAND_ENV_03', name: 'Madhav Gadgil', ministry: MinistryType.Environment, competence: 0.86, hiringFee: 17_000_000, monthlySalary: 270_000, bio: 'Ecologist renowned for western ghats conservation and ecological zoning.' }
    ],
    [MinistryType.Justice]: [
      { id: 'CAND_JUST_01', name: 'Justice B.N. Srikrishna', ministry: MinistryType.Justice, competence: 0.92, hiringFee: 24_000_000, monthlySalary: 380_000, bio: 'Distinguished jurist known for data privacy law and judicial modernization.' },
      { id: 'CAND_JUST_02', name: 'Fali S. Nariman', ministry: MinistryType.Justice, competence: 0.94, hiringFee: 28_000_000, monthlySalary: 450_000, bio: 'Eminent constitutional jurist with unparalleled mastery of legal ethics.' },
      { id: 'CAND_JUST_03', name: 'Gopal Subramanium', ministry: MinistryType.Justice, competence: 0.83, hiringFee: 16_000_000, monthlySalary: 260_000, bio: 'Senior counsel committed to judicial backlog clearance and legal reform.' }
    ],
    [MinistryType.Culture]: [
      { id: 'CAND_CUL_01', name: 'Kapila Vatsyayan', ministry: MinistryType.Culture, competence: 0.89, hiringFee: 16_000_000, monthlySalary: 250_000, bio: 'Art historian and premier guardian of national cultural heritage.' },
      { id: 'CAND_CUL_02', name: 'Shashi Tharoor', ministry: MinistryType.Culture, competence: 0.87, hiringFee: 18_000_000, monthlySalary: 300_000, bio: 'Literary author and diplomatic cultural ambassador.' },
      { id: 'CAND_CUL_03', name: 'Dr. Karan Singh', ministry: MinistryType.Culture, competence: 0.84, hiringFee: 14_000_000, monthlySalary: 220_000, bio: 'Statesman promoting interfaith dialogue and philosophical traditions.' }
    ],
    [MinistryType.Tourism]: [
      { id: 'CAND_TOUR_01', name: 'Deep Kalra', ministry: MinistryType.Tourism, competence: 0.88, hiringFee: 18_000_000, monthlySalary: 300_000, bio: 'Digital travel pioneer transforming international tourist acquisition.' },
      { id: 'CAND_TOUR_02', name: 'Priya Paul', ministry: MinistryType.Tourism, competence: 0.82, hiringFee: 14_000_000, monthlySalary: 220_000, bio: 'Hospitality innovator promoting boutique luxury and eco-tourism corridors.' },
      { id: 'CAND_TOUR_03', name: 'Amitabh Kant', ministry: MinistryType.Tourism, competence: 0.91, hiringFee: 24_000_000, monthlySalary: 390_000, bio: 'Branding mastermind behind iconic global tourism campaigns.' }
    ],
    [MinistryType.Sports]: [
      { id: 'CAND_SPT_01', name: 'Abhinav Bindra', ministry: MinistryType.Sports, competence: 0.93, hiringFee: 22_000_000, monthlySalary: 350_000, bio: 'Olympic gold medalist and athletic high-performance training architect.' },
      { id: 'CAND_SPT_02', name: 'Pullela Gopichand', ministry: MinistryType.Sports, competence: 0.90, hiringFee: 20_000_000, monthlySalary: 320_000, bio: 'Legendary coach championing grassroots sports academies and physical culture.' },
      { id: 'CAND_SPT_03', name: 'Mary Kom', ministry: MinistryType.Sports, competence: 0.85, hiringFee: 16_000_000, monthlySalary: 250_000, bio: 'World champion athlete inspiring youth fitness and boxing infrastructure.' }
    ]
  };

  static getCandidates(ministry: MinistryType): MinisterCandidate[] {
    return this.CANDIDATES_BY_MINISTRY[ministry] || [];
  }
}
