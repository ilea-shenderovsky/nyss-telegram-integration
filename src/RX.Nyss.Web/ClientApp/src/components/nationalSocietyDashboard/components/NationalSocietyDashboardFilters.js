import styles from "./NationalSocietyDashboardFilters.module.scss";
import React, { useState } from 'react';
import { DatePicker } from "../../forms/DatePicker";
import { strings, stringKeys } from "../../../strings";
import {
  useMediaQuery,
  LinearProgress,
  Chip,
  IconButton,
  Grid,
  TextField,
  MenuItem,
  Card,
  CardContent,
  CardHeader,
  FormControl,
  FormLabel,
  RadioGroup,
  FormControlLabel,
  Radio,
  Checkbox,
} from "@material-ui/core";
import DateRange from "@material-ui/icons/DateRange";
import ExpandMore from "@material-ui/icons/ExpandMore";
import { ConditionalCollapse } from "../../common/conditionalCollapse/ConditionalCollapse";
import { convertToLocalDate, convertToUtc } from "../../../utils/date";
import { Fragment } from "react";
import { ReportStatusFilter } from "../../common/filters/ReportStatusFilter";
import { DataConsumer } from "../../../authentication/roles";
import MultiSelectField from "../../forms/MultiSelectField";
import LocationFilter from "../../common/filters/LocationFilter";
import { renderFilterLabel } from "../../common/filters/logic/locationFilterService";

export const NationalSocietyDashboardFilters = ({ filters, healthRisks, organizations, locations, onChange, isFetching, userRoles, rtl }) => {
  const [value, setValue] = useState(filters);
  const isSmallScreen = useMediaQuery(theme => theme.breakpoints.down('lg'));
  const [isFilterExpanded, setIsFilterExpanded] = useState(false);

  const updateValue = (change) => {
    const newValue = {
      ...value,
      ...change
    }

    setValue(newValue);
    return newValue;
  };

  const collectionsTypes = {
    "all": strings(stringKeys.dashboard.filters.allReportsType),
    "dataCollector": strings(stringKeys.dashboard.filters.dataCollectorReportsType),
    "dataCollectionPoint": strings(stringKeys.dashboard.filters.dataCollectionPointReportsType)
  }

  const handleLocationChange = (locations) => {
    onChange(updateValue({ locations: locations }));
  }
  const handleHealthRiskChange = event =>
    onChange(updateValue({ healthRisks: typeof event.target.value === 'string' ? event.target.value.split(',') : event.target.value }))

  const handleOrganizationChange = event =>
    onChange(updateValue({ organizationId: event.target.value === 0 ? null : event.target.value }))

  const handleDateFromChange = date =>
    onChange(updateValue({ startDate: convertToUtc(date) }))

  const handleDateToChange = date =>
    onChange(updateValue({ endDate: convertToUtc(date) }))

  const handleGroupingTypeChange = event =>
    onChange(updateValue({ groupingType: event.target.value }))

  const handleDataCollectorTypeChange = event =>
    onChange(updateValue({ dataCollectorType: event.target.value }))

  const handleReportStatusChange = event =>
    onChange(updateValue({ reportStatus: { ...value.reportStatus, [event.target.name]: event.target.checked } }));

  const renderHealthRiskValues = (selectedIds) => 
    selectedIds.length < 1 || selectedIds.length === healthRisks.length
      ? strings(stringKeys.dashboard.filters.healthRiskAll)
      : selectedIds.map(id => healthRisks.find(hr => hr.id === id).name).join(',');
  const allLocationsSelected = () => !value.locations || value.locations.regionIds.length === locations.regions.length;

  const renderLocationLabel = () => 
    !locations
      ? strings(stringKeys.filters.area.all) 
      : renderFilterLabel(value.locations, locations.regions, false);

  if (!value) {
    return null;
  }

  return (
    <Card className={styles.filters}>
      {isFetching && (<LinearProgress color="primary" />)}
      {isSmallScreen && (
        <CardContent className={styles.collapsedFilterBar} >
          <Grid container spacing={2} alignItems="center">
            <Grid item>
              <CardHeader title={strings(stringKeys.dashboard.filters.title)}/>
            </Grid>
            {!isFilterExpanded && (
              <Fragment>
                <Grid item>
                  <Chip icon={<DateRange/>}
                        label={`${convertToLocalDate(value.startDate).format('YYYY-MM-DD')} - ${convertToLocalDate(value.endDate).format('YYYY-MM-DD')}`}
                        onClick={() => setIsFilterExpanded(!isFilterExpanded)}/>
                </Grid>
                <Grid item>
                  <Chip label={
                    value.groupingType === "Day" ? strings(stringKeys.dashboard.filters.timeGroupingDay) : strings(stringKeys.dashboard.filters.timeGroupingWeek)
                  } onClick={() => setIsFilterExpanded(!isFilterExpanded)}/>
                </Grid>
              </Fragment>
            )}
            {!isFilterExpanded && !allLocationsSelected() && (
              <Grid item>
                <Chip label={renderLocationLabel()}
                      onClick={() => setIsFilterExpanded(!isFilterExpanded)}/>
              </Grid>
            )}
            {!isFilterExpanded && value.healthRiskId && (
              <Grid item>
                <Chip label={healthRisks.filter(hr => hr.id === value.healthRiskId)[0].name}
                      onDelete={() => onChange(updateValue({healthRiskId: null}))}
                      onClick={() => setIsFilterExpanded(!isFilterExpanded)}/>
              </Grid>
            )}
            {!isFilterExpanded && value.dataCollectorType !== "all" && (
              <Grid item>
                <Chip label={collectionsTypes[value.dataCollectorType]}
                      onDelete={() => onChange(updateValue({dataCollectorType: "all"}))}
                      onClick={() => setIsFilterExpanded(!isFilterExpanded)}/>
              </Grid>
            )}
            {!isFilterExpanded && value.organizationId && (
              <Grid item>
                <Chip label={organizations.filter(o => o.id === value.organizationId)[0].name}
                      onDelete={() => onChange(updateValue({organizationId: null}))}
                      onClick={() => setIsFilterExpanded(!isFilterExpanded)}/>
              </Grid>
            )}
            {!isFilterExpanded && !userRoles.some(r => r === DataConsumer) && value.reportStatus.kept && (
              <Grid item>
                <Chip label={strings(stringKeys.filters.report.kept)} onDelete={() => onChange(updateValue({
                  reportStatus: {
                    ...value.reportStatus,
                    kept: false
                  }
                }))} onClick={() => setIsFilterExpanded(!isFilterExpanded)}/>
              </Grid>
            )}
            {!isFilterExpanded && !userRoles.some(r => r === DataConsumer) && value.reportStatus.notCrossChecked && (
              <Grid item>
                <Chip label={strings(stringKeys.filters.report.notCrossChecked)}
                      onDelete={() => onChange(updateValue({
                        reportStatus: {
                          ...value.reportStatus,
                          notCrossChecked: false
                        }
                      }))} onClick={() => setIsFilterExpanded(!isFilterExpanded)}/>
              </Grid>
            )}
            <Grid item className={`${styles.expandFilterButton} ${rtl ? styles.rtl : ''}`}>
              <IconButton data-expanded={isFilterExpanded} onClick={() => setIsFilterExpanded(!isFilterExpanded)}>
                <ExpandMore/>
              </IconButton>
            </Grid>
          </Grid>
        </CardContent>
      )}
      <ConditionalCollapse collapsible={isSmallScreen} expanded={isFilterExpanded}>
        {!isSmallScreen && (
          <Grid container spacing={2}>
            <CardHeader title={strings(stringKeys.dashboard.filters.title)} className={styles.filterTitle} />
          </Grid>
        )}
        <CardContent>
          <Grid container spacing={2}>
            <Grid item>
              <DatePicker
                className={styles.filterDate}
                onChange={handleDateFromChange}
                label={strings(stringKeys.dashboard.filters.startDate)}
                value={convertToLocalDate(value.startDate)}
              />
            </Grid>

            <Grid item>
              <DatePicker
                className={styles.filterDate}
                onChange={handleDateToChange}
                label={strings(stringKeys.dashboard.filters.endDate)}
                value={convertToLocalDate(value.endDate)}
              />
            </Grid>

            <Grid item>
              <FormControl>
                <FormLabel component='legend'>{strings(stringKeys.dashboard.filters.timeGrouping)}</FormLabel>
                <RadioGroup value={value.groupingType} onChange={handleGroupingTypeChange} className={styles.radioGroup}>
                  <FormControlLabel
                    className={styles.radio}
                    label={strings(stringKeys.dashboard.filters.timeGroupingDay)}
                    value={'Day'}
                    control={<Radio color='primary' />}
                  />
                  <FormControlLabel
                    className={styles.radio}
                    label={strings(stringKeys.dashboard.filters.timeGroupingWeek)}
                    value={'Week'}
                    control={<Radio color='primary' />}
                  />
                </RadioGroup>
              </FormControl>
            </Grid>

            <Grid item>
              <LocationFilter 
                value={value.locations}
                filterLabel={renderLocationLabel()}
                locations={locations}
                onChange={handleLocationChange}
                rtl={rtl} />
            </Grid>

            <Grid item>
              <MultiSelectField
                name="healthRisks"
                label={strings(stringKeys.dashboard.filters.healthRisk)}
                onChange={handleHealthRiskChange}
                value={value.healthRisks}
                renderValues={renderHealthRiskValues}
                className={styles.filterItem}
              >
                {healthRisks.map(healthRisk => (
                  <MenuItem key={`filter_healthRisk_${healthRisk.id}`} value={healthRisk.id}>
                    <Checkbox checked={value.healthRisks.indexOf(healthRisk.id) > -1} />
                    <span>{healthRisk.name}</span>
                  </MenuItem>
                ))}
              </MultiSelectField>
            </Grid>

            <Grid item>
              <TextField
                select
                label={strings(stringKeys.dashboard.filters.reportsType)}
                onChange={handleDataCollectorTypeChange}
                value={value.reportsType || "all"}
                className={styles.filterItem}
                InputLabelProps={{ shrink: true }}
              >
                <MenuItem value="all">
                  {collectionsTypes["all"]}
                </MenuItem>
                <MenuItem value="dataCollector">
                  {collectionsTypes["dataCollector"]}
                </MenuItem>
                <MenuItem value="dataCollectionPoint">
                  {collectionsTypes["dataCollectionPoint"]}
                </MenuItem>
              </TextField>
            </Grid>

            {organizations.length > 1 && (
              <Grid item>
                <TextField
                  select
                  label={strings(stringKeys.dashboard.filters.organization)}
                  onChange={handleOrganizationChange}
                  value={value.organizationId || 0}
                  className={styles.filterItem}
                  InputLabelProps={{ shrink: true }}
                >
                  <MenuItem value={0}>{strings(stringKeys.dashboard.filters.organizationsAll)}</MenuItem>

                  {organizations.map(organization => (
                    <MenuItem key={`filter_organization_${organization.id}`} value={organization.id}>
                      {organization.name}
                    </MenuItem>
                  ))}
                </TextField>
              </Grid>
            )}

            {!userRoles.some(r => r === DataConsumer) && (
              <Grid item>
                <ReportStatusFilter
                  filter={value.reportStatus}
                  correctReports
                  showDismissedFilter
                  doNotWrap
                  onChange={handleReportStatusChange}
                />
              </Grid>
            )}
          </Grid>
        </CardContent>
      </ConditionalCollapse>
    </Card>
  );
}
