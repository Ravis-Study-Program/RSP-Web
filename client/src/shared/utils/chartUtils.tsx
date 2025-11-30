import { PieLabel } from 'recharts';

/**
 * Creates a custom inside label function for pie charts
 * Conveniance when Mantines PieChart isn't sufficient for what we want
 * @param valueFormatter - Function to format the value displayed in the label
 * @param radiusMultiplier - Multiplier for label position (0.3 = 30% from inner radius, default 0.5 = 50%)
 * @param fontSize - Font size for the label text
 * @returns A PieLabel function for use with recharts Pie component
 */
export const createPieLabels = (
  valueFormatter: (value: number) => string,
  radiusMultiplier: number = 0.3,
  fontSize: number = 16
): PieLabel => {
  return ({ cx, cy, midAngle, innerRadius, outerRadius, value }) => {
    const RADIAN = Math.PI / 180;
    const radius = innerRadius + (outerRadius - innerRadius) * radiusMultiplier;
    const x = cx + radius * Math.cos(-midAngle * RADIAN);
    const y = cy + radius * Math.sin(-midAngle * RADIAN);

    return (
      <text
        x={x}
        y={y}
        textAnchor={x > cx ? 'start' : 'end'}
        dominantBaseline="central"
        style={{ fontSize }}
      >
        {valueFormatter(value)}
      </text>
    );
  };
};

