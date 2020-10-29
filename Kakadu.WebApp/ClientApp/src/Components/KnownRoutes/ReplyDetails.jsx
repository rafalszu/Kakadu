import React from "react";
import { connect } from "react-redux";
import { Formik, Field, ErrorMessage } from 'formik';
import * as Yup from 'yup';

import { Tabs, Tab } from '../Tabs';
import TextField from "../../UIComponents/TextField";

class ReplyDetails extends React.Component {
    render() {
        const {knownRouteReply} = this.props;
        const requiredFieldMessage = 'This field is required';
        const ReplyDetailsSchema = Yup.object({
            contentType: Yup.string().required(requiredFieldMessage),
            headers: Yup.array(),
            contentRaw: Yup.string(),
        });

        const replyDetailsInitialValues = { 
            id: knownRouteReply.id || '',
            statusCode: knownRouteReply.statusCode || '',
            headers: knownRouteReply.headers || [],
            contentTypeString: knownRouteReply.contentTypeString || '',
            contentTypeCharset: knownRouteReply.contentTypeCharset || '',
            contentType: knownRouteReply.contentType || '',
            contentRaw: knownRouteReply.contentRaw || '',
            contentLength: knownRouteReply.contentLength || null,
            contentEncoding: knownRouteReply.contentEncoding || '',
            contentBase64: knownRouteReply.contentBase64 || ''
        };

        return (
            <Formik
                enableReinitialize
                initialValues={replyDetailsInitialValues}
                validationSchema={ReplyDetailsSchema}
                >
                {({ errors,
                touched,
                handleSubmit,
                isSubmitting,
                values }) => (
                    <div className="col-md-12">
                        <h5>Reply details</h5>
                        <div className="form-row">
                            <div className="form-group col-md-4">
                                <TextField
                                    name="contentType"
                                    label="Content Type"
                                    placeholder="content type"
                                    renderErrorMessage
                                    errors
                                    touched
                                />
                            </div>
                            <div className="form-group col-md-4">
                                <TextField
                                    name="contentTypeCharset"
                                    label="Content Type Charset"
                                    placeholder="content type charset"
                                    renderErrorMessage
                                    errors
                                    touched
                                    disabled
                                />
                            </div>
                            <div className="form-group col-md-4">
                                <TextField
                                    name="contentEncoding"
                                    label="Content Encoding"
                                    placeholder="content encoding"
                                    renderErrorMessage
                                    errors
                                    touched
                                    disabled
                                />
                            </div>
                        </div>
                        <Tabs
                            activeTab={{
                                id: "headers"
                            }}
                            >
                            <Tab id="headers" title="Headers">
                                <div style={{ padding: 10 }}>stored response headers</div>
                            </Tab>
                            <Tab id="body" title="Response body">
                                <div style={{ paddingTop: 10 }}>This is response body</div>
                            </Tab>
                        </Tabs>
                    </div>
                )}
            </Formik>
        )
    }
}

function mapStateToProps(state) {
    const { knownRouteReply } = state;
    return {
        knownRouteReply
    }
}

const connectedKnownRouteReplyDetailsPage = connect(mapStateToProps)(ReplyDetails);
export { connectedKnownRouteReplyDetailsPage as ReplyDetails };
