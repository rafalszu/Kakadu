import React from 'react';
import { Router, Route } from 'react-router-dom';
import { connect } from 'react-redux';

import { history } from '../_helpers';
import { alertActions } from '../_actions';
import { PrivateRoute } from '../Components';
import { Home } from '../Components/Home';
import { Login } from '../Components/Login';
import { Layout } from '../Components/Layout';

import '../custom.css'

class App extends React.Component {
    constructor(props) {
        super(props);

        const { dispatch } = this.props;
        history.listen((location, action) => {
            // clear alert on location change
            dispatch(alertActions.clear());
        });
    }

    render() {
        const { alert } = this.props;
        return (
            // <Layout>
            //     {alert.message &&
            //         <div className={`alert ${alert.type}`}>{alert.message}</div>
            //     }
            //     <Router history={history}>
            //         <div>
            //             <PrivateRoute exact path="/" component={HomePage} />
            //             <Route path="/login" component={LoginPage} />
            //         </div>
            //     </Router>
            // </Layout>
            <Router history={history}>
                <Layout>
                    {alert.message &&
                        <div className={`alert ${alert.type}`}>{alert.message}</div>
                    }
                    <div>
                        <PrivateRoute exact path="/" component={Home} />
                        <Route path="/login" component={Login} />
                    </div>
                </Layout>
            </Router>
            // <div className="jumbotron">
            //     <div className="container">
            //         <div className="col-sm-8 col-sm-offset-2">
            //             {alert.message &&
            //                 <div className={`alert ${alert.type}`}>{alert.message}</div>
            //             }
            //             <Router history={history}>
            //                 <div>
            //                     <PrivateRoute exact path="/" component={HomePage} />
            //                     <Route path="/login" component={LoginPage} />
            //                 </div>
            //             </Router>
            //         </div>
            //     </div>
            // </div>
        );
    }
}

function mapStateToProps(state) {
    const { alert } = state;
    return {
        alert
    };
}

const connectedApp = connect(mapStateToProps)(App);
export { connectedApp as App }; 