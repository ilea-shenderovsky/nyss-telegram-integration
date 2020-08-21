import React from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import * as translationsActions from './logic/translationsActions';
import { useLayout } from '../../utils/layout';
import Layout from '../layout/Layout';
import TranslationsTable from './TranslationsTable';
import { useMount } from '../../utils/lifecycle';

const EmailTranslationsListPageComponent = (props) => {
  useMount(() => {
    props.openTranslationsList(props.match.path, props.match.params);
  });

  return (
    <TranslationsTable
      isListFetching={props.isListFetching}
      languages={props.languages}
      translations={props.translations}
      type="email"
    />
  );
}

EmailTranslationsListPageComponent.propTypes = {
  isFetching: PropTypes.bool,
  languages: PropTypes.array,
  translations: PropTypes.array
};

const mapStateToProps = (state) => ({
  isListFetching: state.translations.listFetching,
  translations: state.translations.emailTranslations,
  languages: state.translations.emailLanguages
});

const mapDispatchToProps = {
  openTranslationsList: translationsActions.openEmailTranslationsList.invoke
};

export const EmailTranslationsListPage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(EmailTranslationsListPageComponent)
);