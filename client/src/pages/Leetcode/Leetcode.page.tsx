import { LineChart } from '@mantine/charts';
import { Container } from '@mantine/core';
import { Layout } from '@/components/Layout/Layout';
import { mockProblems, transformData } from './Leetcode.data';
import LeetcodeTable from './LeetcodeTable/LeetcodeTable';
import classes from './Leetcode.module.css';

export function LeetcodePage() {
  return (
    <Layout>
      <Container fluid className={classes.graphContainer}>
        <LineChart
          h={300}
          data={transformData(mockProblems)}
          dataKey="date"
          series={[
            { name: 'Easy', color: 'green.6' },
            { name: 'Medium', color: 'yellow.6' },
            { name: 'Hard', color: 'red.6' },
          ]}
          curveType="linear"
          tickLine="xy"
          gridAxis="xy"
          yAxisProps={{ domain: [0], minTickGap: 1 }}
          withLegend
          legendProps={{ verticalAlign: 'top', height: 50 }}
        />
      </Container>
      <LeetcodeTable />
    </Layout>
  );
}
