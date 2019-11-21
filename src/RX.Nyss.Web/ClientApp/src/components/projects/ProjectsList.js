import React from 'react';
import PropTypes from "prop-types";
import Grid from '@material-ui/core/Grid';
import Card from '@material-ui/core/Card';
import CardActions from '@material-ui/core/CardActions';
import CardContent from '@material-ui/core/CardContent';
import Typography from '@material-ui/core/Typography';
import Button from '@material-ui/core/Button';
import ArrowForwardIcon from '@material-ui/icons/ArrowForward';
import { Loading } from '../common/loading/Loading';
import { strings, stringKeys } from '../../strings';
import dayjs from "dayjs"
import styles from "./ProjectsList.module.scss";
import { Box } from '@material-ui/core';

export const ProjectsList = ({ isListFetching, isRemoving, goToEdition, goToDashboard, remove, list, nationalSocietyId }) => {
  if (isListFetching) {
    return <Loading />;
  }

  return (
    <Grid container spacing={3} style={{ maxWidth: 1200 }}>
      {list.map(project => (
        <Grid key={`projectsListItem_${project.id}`} item xs={6}>
          <Card key={project.id} className={styles.projectCard}>
            <CardContent>
              <Box b={3}>
                <Typography variant="h3">
                  {project.name}
                </Typography>
              </Box>
              <Grid container spacing={3}>
                <Grid item container xs={6} direction="column" spacing={1}>
                  <Grid item>
                    <Box className={styles.keyPoint}>
                      <Typography variant="h6">
                        {project.totalReportCount}
                      </Typography>
                      <Typography variant="body1" color="textSecondary" gutterBottom>
                        {strings(stringKeys.project.list.totalReportCount)}
                      </Typography>
                    </Box>
                    <Box className={styles.keyPoint}>
                      <Typography variant="h6">
                        {project.activeDataCollectorCount}
                      </Typography>
                      <Typography variant="body1" color="textSecondary" gutterBottom>
                        {strings(stringKeys.project.list.activeDataCollectorCount)}
                      </Typography>
                    </Box>
                    <Box className={styles.keyPoint}>
                      <Typography variant="h6">
                        {dayjs(project.startDate).format("YYYY-MM-DD")}
                      </Typography>
                      <Typography variant="body1" color="textSecondary" gutterBottom>
                        {strings(stringKeys.project.list.startDate)}
                      </Typography>
                    </Box>
                  </Grid>
                </Grid>
                <Grid item container xs={6} direction="column" spacing={1}>
                  <Grid item>
                    <Box className={styles.keyPoint}>
                      <Typography variant="h6">
                        {project.escalatedAlertCount}
                      </Typography>
                      <Typography variant="body1" color="textSecondary" gutterBottom>
                        {strings(stringKeys.project.list.escalatedAlertCount)}
                      </Typography>
                    </Box>
                    <Box className={styles.keyPoint}>
                      <Typography variant="h6">
                        {project.supervisorCount}
                      </Typography>
                      <Typography variant="body1" color="textSecondary" gutterBottom>
                        {strings(stringKeys.project.list.supervisorCount)}
                      </Typography>
                    </Box>
                    <Box className={styles.keyPoint}>
                      <Typography variant="h6">
                        {project.endDate ? dayjs(project.endDate).format("YYYY-MM-DD") : strings(stringKeys.project.list.ongoing)}
                      </Typography>
                      <Typography variant="body1" color="textSecondary" gutterBottom>
                        {strings(stringKeys.project.list.endDate)}
                      </Typography>
                    </Box>
                  </Grid>
                </Grid>
              </Grid>
            </CardContent>
            <CardActions>
              <Grid container direction="row" justify="flex-end" alignItems="baseline" spacing={3}>
                <Grid item>
                  <Button onClick={() => goToDashboard(nationalSocietyId, project.id)} endIcon={<ArrowForwardIcon />}>{strings(stringKeys.project.list.open)}</Button>
                </Grid>
              </Grid>
            </CardActions>
          </Card>
        </Grid>
      ))}
    </Grid>
  );
}

ProjectsList.propTypes = {
  isFetching: PropTypes.bool,
  list: PropTypes.array
};

export default ProjectsList;
