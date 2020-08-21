import React, { useState, Fragment } from 'react';
import Button from '@material-ui/core/Button';
import Dialog from '@material-ui/core/Dialog';
import DialogActions from '@material-ui/core/DialogActions';
import DialogContent from '@material-ui/core/DialogContent';
import DialogTitle from '@material-ui/core/DialogTitle';
import { createForm } from '../../../utils/forms';
import TextInputField from '../../forms/TextInputField';
import useMediaQuery from '@material-ui/core/useMediaQuery';
import { post, get } from '../../../utils/http';
import { useMount } from '../../../utils/lifecycle';
import { Loading } from '../loading/Loading';
import { updateStrings } from '../../../strings';
import Grid from '@material-ui/core/Grid';
import { useDispatch } from 'react-redux';
import { stringsUpdated } from '../../app/logic/appActions';
import TextWithHTMLPreviewInputField from '../../forms/TextInputWithHTMLPreviewField';
import { useTheme } from '@material-ui/core';

export const EmailStringsEditorDialog = ({ stringKey, close }) => {
  const [form, setForm] = useState(null);
  const [languageCodes, setLanguageCodes] = useState([]);
  const dispatch = useDispatch();
  const theme = useTheme();
  const fullScreen = useMediaQuery(theme.breakpoints.down('md'));

  const currentLanguageCode = "en";

  useMount(() => {
    get(`/api/resources/getEmailString/${encodeURI(stringKey)}`)
      .then(response => {
        const translations = response.value.translations;

        setLanguageCodes(translations.map(t => ({ languageCode: t.languageCode, name: t.name, value: t.value })));

        const translationFields = translations.reduce((prev, current) => ({
          ...prev,
          [`value_${current.languageCode}`]: current.value
        }), {});

        const fields = {
          key: stringKey,
          ...translationFields
        }

        setForm(createForm(fields));
      });
  });

  const handleSave = () => {
    if (!form.isValid()) {
      return;
    };

    const values = form.getValues();

    const dto = {
      key: values.key,
      translations: languageCodes.map(lang => ({
        languageCode: lang.languageCode,
        value: values[`value_${lang.languageCode}`]
      }))
    };

    post('/api/resources/saveEmailString', dto)
      .then(() => {
        updateStrings({
          [values.key]: values[`value_${currentLanguageCode}`]
        });

        dispatch(stringsUpdated(dto.key, dto.translations.reduce((prev, current) => ({ ...prev, [current.languageCode]: current.value }), {})));
        close();
      });
  }

  if (!form) {
    return null;
  }

  const handleKeyDown = (e) => {
    e.stopPropagation();

    if (e.key === "Escape") {
      close();
    }
  }

  return (
    <Dialog open={true} onClose={close} onClick={e => e.stopPropagation()} onKeyDown={handleKeyDown} fullScreen={fullScreen} maxWidth="md">
      <DialogTitle id="form-dialog-title">Edit string resource</DialogTitle>
      <DialogContent>
        <Grid container spacing={3}>
          {!form && <Loading />}
          {form && (
            <Fragment>
              <Grid item xs={12}>
                <TextInputField label="Key" name="key" field={form.fields.key} />
              </Grid>

              {languageCodes.map((lang, index) => (
                <Grid item xs={12} key={`lang_${lang.languageCode}`}>
                  <TextWithHTMLPreviewInputField
                    autoFocus={index === 0}
                    multiline
                    label={lang.name}
                    name={`value_${lang.languageCode}`}
                    field={form.fields[`value_${lang.languageCode}`]}
                  />
                </Grid>
              ))}
            </Fragment>
          )}
        </Grid>
        <br />
      </DialogContent>
      {form && <DialogActions>
        <Button onClick={close} color="primary" variant="outlined">Cancel</Button>
        <Button onClick={handleSave} color="primary" variant="outlined">Save</Button>
      </DialogActions>}
    </Dialog>
  );
}