import React, {Fragment, useCallback, useState} from 'react';
import { stringKeys, strings } from "../../../strings";
import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle, Grid, MenuItem,
  useMediaQuery,
  useTheme
} from "@material-ui/core";
import SelectField from "../../forms/SelectField";
import {useMount} from "../../../utils/lifecycle";
import {createForm, validators} from "../../../utils/forms";
import SubmitButton from "../../forms/submitButton/SubmitButton";
import {connect, useSelector} from "react-redux";
import Form from "../../forms/form/Form";
import {DatePicker} from "../../forms/DatePicker";
import TextInputField from "../../forms/TextInputField";
import dayjs from "dayjs";
import utc from "dayjs/plugin/utc";

export const CreateAlertEventDialog = ({ isOpened, close, alertId, openCreation, create }) => {
  const theme = useTheme();
  const fullScreen = useMediaQuery(theme.breakpoints.down('xs'));
  dayjs.extend(utc);
  const eventTypes = useSelector( state => state.alertEvents.eventTypes);
  const eventSubtypes = useSelector(state => state.alertEvents.eventSubtypes);
  const [filteredSubtypes, setFilteredSubtypes] = useState([])
  const [date, setDate] = useState(dayjs().format('YYYY-MM-DD'));

  useMount(() => {
    openCreation();
  });

  const [form] = useState(() => {
    const fields = {
      eventTypeId: '',
      eventSubtypeId: '',
      time: dayjs().hour(0).minute(0).format('HH:mm'),
      text: ''
    };

    const validation = {
      eventTypeId: [validators.required],
      eventSubtypeId: [validators.requiredWhen(x => eventSubtypes.some(subtype => x.eventTypeId === subtype.eventTypeId))],
      date: [validators.required],
    };
    return createForm(fields, validation);
  });

  const onEventTypeChange = (event) => {
    const eventTypeId = event.target.value;
    setFilteredSubtypes(eventSubtypes.filter(subtype => subtype.typeId.toString() === eventTypeId))
  }

  const handleDateChange = date => {
    setDate(date.format('YYYY-MM-DD'));
  }

  const handleSubmit = (event) => {
    event.preventDefault();

    if (!form.isValid()) {
      return;
    }

    const values = form.getValues();

    create( alertId,
      {
        eventTypeId: parseInt(values.eventTypeId),
        eventSubtypeId: parseInt(values.eventSubtypeId),
        timestamp: dayjs(`${date} ${values.time}`).utc(),
        text: values.text,
      });

    close();
  };

  return (
    <Dialog onClose={close} open={isOpened} fullScreen={fullScreen}>

      <DialogTitle id="form-dialog-title">
        {strings(stringKeys.alerts.logs.addNew)}
      </DialogTitle>

      <DialogContent>
        <Form onSubmit={handleSubmit} fullWidth>
          <Grid container spacing={2}>
            <Grid item xs={12}>
              <SelectField
                label={strings(stringKeys.alerts.logs.list.type)}
                name="type"
                field={form.fields.eventTypeId}
                onChange={onEventTypeChange}
              >
                {eventTypes.map(type => (
                  <MenuItem
                    value={type.id.toString()}
                    key={type.id}
                  >
                    {strings(stringKeys.alerts.constants.eventTypes[type.name])}
                  </MenuItem>
                ))}
              </SelectField>
            </Grid>

            <Grid item xs={12}>
              <SelectField
                label={strings(stringKeys.alerts.logs.list.subtype)}
                name="subtype"
                field={form.fields.eventSubtypeId}
              >
                {filteredSubtypes.map(type => (
                  <MenuItem
                    value={type.id.toString()}
                    key={type.id}
                  >
                    {strings(stringKeys.alerts.constants.eventSubtypes[type.name])}
                  </MenuItem>
                ))}
              </SelectField>
            </Grid>

            <Grid>
              <DatePicker
                label={strings(stringKeys.alerts.logs.form.dateOfEvent)}
                fullWidth
                onChange={handleDateChange}
                value={date}
              />
            </Grid>

            <Grid item xs={6}>
              <TextInputField
                label={strings(stringKeys.alerts.logs.form.timeOfEvent)}
                type="time"
                name="time"
                field={form.fields.time}
                pattern="[0-9]{2}:[0-9]{2}"
              />
            </Grid>

            <Grid item xs={6}>
              <TextInputField
                label={strings(stringKeys.alerts.logs.form.text)}
                type="text"
                name="text"
                field={form.fields.text}
              />
            </Grid>

          </Grid>

          <DialogActions>
            <Button onClick={close}>
              {strings(stringKeys.form.cancel)}
            </Button>
            <SubmitButton>
              {strings(stringKeys.alerts.logs.addNew)}
            </SubmitButton>
          </DialogActions>
        </Form>
      </DialogContent>

    </Dialog>
  );
}