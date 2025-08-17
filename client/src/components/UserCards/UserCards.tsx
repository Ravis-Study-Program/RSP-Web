import { useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import {
  Avatar,
  Badge,
  Button,
  Card,
  Flex,
  Grid,
  MultiSelect,
  Skeleton,
  Text,
} from '@mantine/core';
import { EnrollmentUserDto, SeasonRole, SeasonStudentRolePromotion } from '@/generated/api/client';
import {
  SeasonRoleReverseIndex,
  SeasonStudentRolePromotionReverseIndex,
} from '@/shared/entities/reverseIndex';
import { createOptionsFilter, getUsers } from '@/shared/table/globalFilters';
import { SeasonStudentRolePromotionColors } from '@/shared/utils/colorMappings';
import classes from './UserCards.module.css';

export function useFilterUsers(users: EnrollmentUserDto[] = []) {
  const [selectedNames, setSelectedNames] = useState<string[]>([]);
  const [selectedRoles, setSelectedRoles] = useState<number[]>([]);
  const [selectedPromotions, setSelectedPromotions] = useState<number[]>([]);

  const filtered = useMemo(() => {
    return users.filter((user) => {
      const nameMatch =
        selectedNames.length === 0 || (user.name != null && selectedNames.includes(user.name));

      const roleMatch =
        selectedRoles.length === 0 || (user.role != null && selectedRoles.includes(user.role));

      const promotionMatch =
        selectedPromotions.length === 0 ||
        (user.studentRolePromotion != null &&
          selectedPromotions.includes(user.studentRolePromotion));

      return nameMatch && roleMatch && promotionMatch;
    });
  }, [users, selectedNames, selectedRoles, selectedPromotions]);

  return {
    selectedNames,
    setSelectedNames,
    selectedRoles,
    setSelectedRoles,
    selectedPromotions,
    setSelectedPromotions,
    filtered,
  };
}

type Props = {
  allUsers: EnrollmentUserDto[];
  selectedNames: string[];
  selectedRoles: number[];
  selectedPromotions: number[];
  onChangeName: (names: string[]) => void;
  onChangeRole: (roles: number[]) => void;
  onChangePromotion: (promotions: number[]) => void;
  filteredCount: number;
};

export function UserFilterPanel({
  allUsers,
  selectedNames,
  selectedRoles,
  selectedPromotions,
  onChangeName,
  onChangeRole,
  onChangePromotion,
  filteredCount,
}: Props) {
  const userOptions = getUsers(allUsers);

  const roleOptions = Array.from(new Set(allUsers.map((u) => u.role).filter((r) => r != null))).map(
    (role) => ({
      value: role.toString(),
      label: SeasonRoleReverseIndex[role],
    })
  );

  const promotionOptions = Array.from(
    new Set(allUsers.map((u) => u.studentRolePromotion).filter((p) => p !== 0 && p != null))
  ).map((promo) => ({
    value: promo.toString(),
    label: SeasonStudentRolePromotionReverseIndex[promo],
  }));

  return (
    <Flex mb="xl" pb="xl" gap="md" wrap="wrap" justify="space-between" align="center">
      <Flex gap="md" wrap="wrap" align="flex-end">
        <MultiSelect
          classNames={{ inputField: classes.inputField }}
          label="Name"
          placeholder="Pick value(s)"
          data={userOptions}
          filter={createOptionsFilter()}
          miw={150}
          searchable
          nothingFoundMessage="Nothing found..."
          value={selectedNames}
          onChange={(values) => onChangeName(values as string[])}
        />
        {roleOptions.length > 0 && promotionOptions.length > 0 && (
          <>
            <MultiSelect
              label="Role"
              placeholder="Select role(s)"
              data={roleOptions}
              value={selectedRoles.map(String)}
              onChange={(values) => onChangeRole(values.map(Number))}
            />

            <MultiSelect
              label="Promotion"
              placeholder="Select promotion(s)"
              data={promotionOptions}
              value={selectedPromotions.map(String)}
              onChange={(values) => onChangePromotion(values.map(Number))}
            />
          </>
        )}
      </Flex>
      <Text size="sm" mt="xs">
        {filteredCount} found
      </Text>
    </Flex>
  );
}

type UserCardsProps = {
  users: EnrollmentUserDto[] | undefined;
};

export function UserCards({ users }: UserCardsProps) {
  const formatNameByRole = (user: EnrollmentUserDto) => {
    let prefix = '';

    switch (user.role) {
      case SeasonRole.Coordinator:
        prefix = '👑';
        break;
      case SeasonRole.Mentor:
        prefix = '💎';
        break;
      default:
        break;
    }

    return `${prefix} ${user.name}`;
  };

  return (
    <>
      {users?.map((user, key) => (
        <Grid.Col key={key} span={{ base: 12, sm: 6, md: 6, lg: 2 }}>
          <Card withBorder shadow="sm" radius="md" py="xl" className={classes.card}>
            <Avatar
              color="initials"
              name={user?.name || undefined}
              size={60}
              radius={60}
              mx="auto"
            />
            <Text ta="center" fz="lg" fw={600} size="sm" mt="md">
              {formatNameByRole(user)}
            </Text>

            {(user.role != null || user.studentRolePromotion != null) && (
              <Flex justify="center" align="center" wrap="wrap" gap="sm" mt={20} mb={10}>
                {user.role != null && (
                  <Badge size="md" variant="light" autoContrast color="gray">
                    {SeasonRoleReverseIndex[user.role]}
                  </Badge>
                )}

                {user.studentRolePromotion != null &&
                  user.studentRolePromotion !== SeasonStudentRolePromotion.NotApplicable && (
                    <Badge
                      size="md"
                      variant="light"
                      color={SeasonStudentRolePromotionColors[user.studentRolePromotion]}
                    >
                      {SeasonStudentRolePromotionReverseIndex[user.studentRolePromotion]}
                    </Badge>
                  )}
              </Flex>
            )}

            <Button
              component={Link}
              to={`/profile?user=${user.slug}`}
              radius="md"
              mt="sm"
              size="xs"
              variant="primary"
            >
              Profile
            </Button>
          </Card>
        </Grid.Col>
      ))}
    </>
  );
}

export function UserSkeletonCards() {
  const numCards = 8;

  return (
    <>
      {Array.from({ length: numCards }).map((_, index) => (
        <Grid.Col key={index} span={{ base: 12, sm: 6, md: 6, lg: 2 }}>
          <Card withBorder shadow="xs" radius="md">
            <Skeleton height={80} mb="xl" />
            <Skeleton height={10} radius="xl" />
            <Skeleton height={8} mt={8} radius="xl" />
            <Skeleton height={8} mt={8} radius="xl" />
            <Skeleton height={8} mt={8} width="70%" radius="xl" />
            <Skeleton height={40} mt={50} radius="xl" />
          </Card>
        </Grid.Col>
      ))}
    </>
  );
}
