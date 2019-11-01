import React, { useEffect, useState, Fragment } from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { useLayout } from '../../utils/layout';
import { validators, createForm } from '../../utils/forms';
import * as smsGatewaysActions from './logic/smsGatewaysActions';
import Layout from '../layout/Layout';
import Form from '../forms/form/Form';
import FormActions from '../forms/formActions/FormActions';
import SubmitButton from '../forms/submitButton/SubmitButton';
import Typography from '@material-ui/core/Typography';
import TextInputField from '../forms/TextInputField';
import SelectInput from '../forms/SelectField';
import MenuItem from "@material-ui/core/MenuItem";
import SnackbarContent from '@material-ui/core/SnackbarContent';
import Button from "@material-ui/core/Button";
import { Loading } from '../common/loading/Loading';
import { SmsGatewayTypes } from "./logic/smsGatewayTypes";
import { useMount } from '../../utils/lifecycle';

const SmsGatewaysEditPageComponent = (props) => {
  const [form, setForm] = useState(null);

  useMount(() => {
    props.openEdition(props.match.path, props.match.params);
  });

  useEffect(() => {
    if (!props.data) {
      return;
    }

    const fields = {
      id: props.data.id,
      name: props.data.name,
      apiKey: props.data.apiKey,
      gatewayType: props.data.gatewayType.toString()
    };

    const validation = {
      name: [validators.required, validators.minLength(1), validators.maxLength(100)],
      apiKey: [validators.required, validators.minLength(1), validators.maxLength(100)],
      gatewayType: [validators.required]
    };

    setForm(createForm(fields, validation));
  }, [props.data, props.match]);

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!form.isValid()) {
      return;
    };

    const values = form.getValues();
    props.edit({
      id: values.id,
      name: values.name,
      apiKey: values.apiKey,
      gatewayType: parseInt(values.gatewayType),
      nationalSocietyId: props.match.params.nationalSocietyId
    });
  };

  if (props.isFetching || !form) {
    return <Loading />;
  }

  return (
    <Fragment>
      <Typography variant="h2">Edit SMS Gateway</Typography>

      {props.error &&
        <SnackbarContent
          message={props.error}
        />
      }

      <Form onSubmit={handleSubmit}>
        <TextInputField
          label="Name"
          name="name"
          field={form.fields.name}
        />

        <TextInputField
          label="API key"
          name="apiKey"
          field={form.fields.apiKey}
        />

        <SelectInput
          label="Gateway type"
          name="gatewayType"
          field={form.fields.gatewayType}
        >
          {Object.keys(SmsGatewayTypes).map(key => (
            <MenuItem
              key={`gatewayType${key}`}
              value={key.toString()}
              selected={form.fields.gatewayType === key.toString()}>
              {SmsGatewayTypes[key]}
            </MenuItem>
          ))}
        </SelectInput>

        <FormActions>
          <Button onClick={() => props.goToList(props.match.params.nationalSocietyId)}>
            Cancel
          </Button>

          <SubmitButton isFetching={props.isSaving}>
            Save SMS Gateway
          </SubmitButton>
        </FormActions>
    </Form>
    </Fragment>
  );
}

SmsGatewaysEditPageComponent.propTypes = {
};

const mapStateToProps = state => ({
  isFetching: state.smsGateways.formFetching,
  isSaving: state.smsGateways.formSaving,
  data: state.smsGateways.formData,
  error: state.smsGateways.formError
});

const mapDispatchToProps = {
  openEdition: smsGatewaysActions.openEdition.invoke,
  edit: smsGatewaysActions.edit.invoke,
  goToList: smsGatewaysActions.goToList  
};

export const SmsGatewaysEditPage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(SmsGatewaysEditPageComponent)
);