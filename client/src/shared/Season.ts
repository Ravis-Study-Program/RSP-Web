export enum SeasonRole {
  Unregistered = 'Unregistered',
  Student = 'Student',
  Mentor = 'Mentor',
  Coordinator = 'Coordinator',
}

export interface Season {
  title: string;
  slug: string;
  role: SeasonRole;
  startDate: Date;
  coordinator: string;
}

export interface SeasonRoleViews {
  student: React.ComponentType;
  mentor: React.ComponentType;
  coordinator: React.ComponentType;
}
