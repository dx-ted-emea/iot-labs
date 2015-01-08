/*
 * Copyright 2012 Nodeable Inc
 *
 *    Licensed under the Apache License, Version 2.0 (the "License");
 *    you may not use this file except in compliance with the License.
 *    You may obtain a copy of the License at
 *
 *        http://www.apache.org/licenses/LICENSE-2.0
 *
 *    Unless required by applicable law or agreed to in writing, software
 *    distributed under the License is distributed on an "AS IS" BASIS,
 *    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *    See the License for the specific language governing permissions and
 *    limitations under the License.
 */

package com.hackathon.storm.Utils;

import backtype.storm.spout.ISpoutOutputCollector;
import backtype.storm.task.IOutputCollector;
import backtype.storm.task.OutputCollector;
import backtype.storm.topology.IBasicOutputCollector;
import backtype.storm.tuple.Tuple;
import backtype.storm.tuple.Values;

import java.util.*;

/**
 * MockOutputCollector implements both {@link backtype.storm.task.IOutputCollector} and {@link backtype.storm.spout.ISpoutOutputCollector}
 * for testing purposes only.
 */
public class MockBasicOutputCollector extends OutputCollector implements backtype.storm.task.IOutputCollector {

    private List<Tuple> ackedTuples = new ArrayList<Tuple>();
    private List<Tuple> failedTuples = new ArrayList<Tuple>();
    private Map<String, List<Values>> emittedValuesMap = new HashMap<String, List<Values>>();
    private Map<String, List<Values>> emittedSpoutValuesMap = new HashMap<String, List<Values>>();
    private Values lastEmmitedValues;

    public MockBasicOutputCollector(IOutputCollector delegate) {
        super(delegate);
    }

    public Map<String, List<Values>> getEmittedValuesMap() {
        return emittedValuesMap;
    }

    public Map<String, List<Values>> getEmittedSpoutValuesMap() {
        return emittedSpoutValuesMap;
    }

    /* Helper methods for non-spout output collection */

    /**
     * Returns the list of values emitted for the stream specified.
     *
     * @param streamId the stream whose values we're interested in
     *
     * @return the list of values emitted to the specific stream to this collector from a non-spout
     */
    public List<Values> getEmittedValuesForStream(String streamId) {
        return emittedValuesMap.get(streamId);
    }

    /**
     * Returns the list of values emitted for all streams from a non-spout.
     *
     * @return the list of values emitted to this collector from a non-spout
     */
    public List<Values> getEmittedValues() {
        List<Values> allValues = new ArrayList<Values>();

        for (Map.Entry<String, List<Values>> entry : emittedValuesMap.entrySet()) {
            allValues.addAll(entry.getValue());
        }

        return allValues;
    }

    /**
     * Returns the last emmited value.
     * @return the last emmited value.
     */
    public Values getLastEmmitedValue() {
        return lastEmmitedValues;
    }

    /**
     * Resets the emitted values.
     */
    public void clearEmittedValues() {
        emittedValuesMap.clear();
        emittedValuesMap = new HashMap<String, List<Values>>();
    }

    /* Helper methods for spout output collection */

    /**
     * Returns the list of values emitted for the stream specified by the spout.
     *
     * @param streamId the stream whose values we're interested in
     *
     * @return the list of values emitted to the specific stream to this collector from a spout
     */
    public List<Values> getEmittedSpoutValuesForStream(String streamId) {
        return emittedValuesMap.get(streamId);
    }

    /**
     * Returns the list of values emitted for all streams by the spout.
     *
     * @return the list of values emitted to this collector from a spout
     */
    public List<Values> getEmittedSpoutValues() {
        List<Values> allValues = new ArrayList<Values>();

        for (Map.Entry<String, List<Values>> entry : emittedSpoutValuesMap.entrySet()) {
            allValues.addAll(entry.getValue());
        }

        return allValues;
    }

    /**
     * Returns the list of acked tuples.
     *
     * @return the list of acked tuples
     */
    public List<Tuple> getAckedTuples() {
        return ackedTuples;
    }
    public List<Tuple> getFailedTuples() {
        return failedTuples;
    }

    /**
     * Resets the emitted spout values.
     */
    public void clearEmittedSpoutValues() {
        emittedSpoutValuesMap.clear();
        emittedSpoutValuesMap = new HashMap<String, List<Values>>();
    }

    /* IOutputCollector Methods */

    /**
     * {@inheritDoc}
     */


    /**
     * {@inheritDoc}
     */


    /**
     * {@inheritDoc}
     */





    /**
     * {@inheritDoc}
     */
    @Override
    public void reportError(Throwable throwable) {
        throw new UnsupportedOperationException("MockObjectCollector#reportError(Throwable) is not implemented!");
    }


    @Override
    public List<Integer> emit(String s, Collection<Tuple> tuples, List<Object> objects) {
        if (!emittedValuesMap.containsKey(s)) {
            emittedValuesMap.put(s, new ArrayList<Values>());
        }

        if (objects != null) {
            lastEmmitedValues = (Values) objects;
            emittedValuesMap.get(s).add((Values)objects);
        }

        return null;
    }

    @Override
    public void emitDirect(int i, String s, Collection<Tuple> tuples, List<Object> objects) {

    }

    @Override
    public void ack(Tuple tuple) {

    }

    @Override
    public void fail(Tuple tuple) {

    }
}